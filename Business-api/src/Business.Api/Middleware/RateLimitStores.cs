using System.Collections.Concurrent;
using StackExchange.Redis;

namespace Business.Api.Middleware;

public sealed class RateLimitingOptions
{
    public const string SectionName = "RateLimiting";
    public bool Enabled { get; set; } = true;
    public string Store { get; set; } = "InMemory";
    public string FailureMode { get; set; } = "Open";
    public string KeyPrefix { get; set; } = "business:rl";
    public string ApplicationCode { get; set; } = "Business";
    public int PolicyCacheSeconds { get; set; } = 30;
    public bool FailOpen => FailureMode.Equals("Open", StringComparison.OrdinalIgnoreCase);
}
public sealed record RateLimitLease(bool Acquired, string Key, string Algorithm, string? LeaseId, int Limit, int Remaining, long ResetAt) { public bool RequiresRelease => Algorithm == RateLimitAlgorithms.Concurrency && LeaseId is not null; }
public static class RateLimitAlgorithms { public const string FixedWindow = "FixedWindow", SlidingWindow = "SlidingWindow", TokenBucket = "TokenBucket", Concurrency = "Concurrency"; public static bool IsSupported(string value) => value is FixedWindow or SlidingWindow or TokenBucket or Concurrency; }
public interface IRateLimitStore { Task<RateLimitLease> AcquireAsync(string key, string algorithm, int permitLimit, int windowSeconds, int burstLimit, CancellationToken token); Task ReleaseAsync(RateLimitLease lease, CancellationToken token); }

public sealed class InMemoryRateLimitStore : IRateLimitStore
{
    private sealed class State { public required string Algorithm { get; init; } public long WindowStart { get; set; } public int Count { get; set; } public Queue<long> Hits { get; } = new(); public double Tokens { get; set; } public long LastRefill { get; set; } public Dictionary<string, long> Leases { get; } = new(); }
    private readonly ConcurrentDictionary<string, State> states = new();
    public Task<RateLimitLease> AcquireAsync(string key, string algorithm, int permit, int seconds, int burst, CancellationToken token)
    {
        token.ThrowIfCancellationRequested(); if (!RateLimitAlgorithms.IsSupported(algorithm)) throw new ArgumentException($"Unsupported rate limit algorithm '{algorithm}'.");
        var capacity = checked(permit + burst); var now = DateTimeOffset.UtcNow; var nowMs = now.ToUnixTimeMilliseconds(); var state = states.AddOrUpdate(key, _ => NewState(algorithm, capacity, nowMs), (_, old) => old.Algorithm == algorithm ? old : NewState(algorithm, capacity, nowMs));
        lock (state)
        {
            if (algorithm == RateLimitAlgorithms.FixedWindow) { var n = now.ToUnixTimeSeconds(); if (n - state.WindowStart >= seconds) { state.WindowStart = n; state.Count = 0; } state.Count++; return Task.FromResult(New(state.Count <= capacity, key, algorithm, null, capacity, capacity - state.Count, state.WindowStart + seconds)); }
            if (algorithm == RateLimitAlgorithms.SlidingWindow) { var n = now.ToUnixTimeSeconds(); while (state.Hits.TryPeek(out var hit) && hit <= n - seconds) state.Hits.Dequeue(); var ok = state.Hits.Count < capacity; if (ok) state.Hits.Enqueue(n); var reset = state.Hits.TryPeek(out var oldest) ? oldest + seconds : n + seconds; return Task.FromResult(New(ok, key, algorithm, null, capacity, capacity - state.Hits.Count, reset)); }
            if (algorithm == RateLimitAlgorithms.TokenBucket) { state.Tokens = Math.Min(capacity, state.Tokens + Math.Max(0, nowMs - state.LastRefill) / 1000d * permit / seconds); state.LastRefill = nowMs; var ok = state.Tokens >= 1; if (ok) state.Tokens--; return Task.FromResult(New(ok, key, algorithm, null, capacity, (int)state.Tokens, now.AddSeconds(Math.Max(0, 1 - state.Tokens) * seconds / permit).ToUnixTimeSeconds())); }
            foreach (var expired in state.Leases.Where(x => x.Value <= nowMs).Select(x => x.Key).ToArray()) state.Leases.Remove(expired); if (state.Leases.Count >= capacity) return Task.FromResult(New(false, key, algorithm, null, capacity, 0, state.Leases.Values.Min() / 1000)); var id = Guid.NewGuid().ToString("N"); state.Leases[id] = now.AddSeconds(seconds).ToUnixTimeMilliseconds(); return Task.FromResult(New(true, key, algorithm, id, capacity, capacity - state.Leases.Count, now.AddSeconds(seconds).ToUnixTimeSeconds()));
        }
    }
    public Task ReleaseAsync(RateLimitLease lease, CancellationToken token) { token.ThrowIfCancellationRequested(); if (lease.RequiresRelease && states.TryGetValue(lease.Key, out var state)) lock (state) state.Leases.Remove(lease.LeaseId!); return Task.CompletedTask; }
    private static State NewState(string algorithm, int capacity, long now) => new() { Algorithm = algorithm, WindowStart = now / 1000, Tokens = capacity, LastRefill = now };
    private static RateLimitLease New(bool ok, string key, string algorithm, string? id, int limit, int remaining, long reset) => new(ok, key, algorithm, id, limit, Math.Max(0, remaining), reset);
}

public sealed class RedisRateLimitStore(IConnectionMultiplexer redis) : IRateLimitStore
{
    private const string AcquireScript = """
        local key=KEYS[1]; local algorithm=ARGV[1]; local permit=tonumber(ARGV[2]); local window=tonumber(ARGV[3]); local burst=tonumber(ARGV[4]); local now=tonumber(ARGV[5]); local leaseId=ARGV[6]; local capacity=permit+burst
        if algorithm=='FixedWindow' then local count=redis.call('INCR',key); if count==1 then redis.call('PEXPIRE',key,window) end; local ttl=redis.call('PTTL',key); return {count<=capacity and 1 or 0,math.max(0,capacity-count),now+ttl,''} end
        if algorithm=='SlidingWindow' then redis.call('ZREMRANGEBYSCORE',key,'-inf',now-window); local count=redis.call('ZCARD',key); local allowed=0; if count<capacity then redis.call('ZADD',key,now,leaseId); count=count+1; allowed=1 end; redis.call('PEXPIRE',key,window); local oldest=redis.call('ZRANGE',key,0,0,'WITHSCORES'); local reset=now+window; if #oldest>0 then reset=tonumber(oldest[2])+window end; return {allowed,math.max(0,capacity-count),reset,''} end
        if algorithm=='TokenBucket' then local values=redis.call('HMGET',key,'tokens','last'); local tokens=tonumber(values[1]) or capacity; local last=tonumber(values[2]) or now; tokens=math.min(capacity,tokens+math.max(0,now-last)*permit/window); local allowed=0; if tokens>=1 then tokens=tokens-1; allowed=1 end; redis.call('HSET',key,'tokens',tokens,'last',now); redis.call('PEXPIRE',key,window*2); local wait=allowed==1 and (window/permit) or ((1-tokens)*window/permit); return {allowed,math.floor(tokens),now+math.max(0,wait),''} end
        if algorithm=='Concurrency' then redis.call('ZREMRANGEBYSCORE',key,'-inf',now); local count=redis.call('ZCARD',key); if count>=capacity then local oldest=redis.call('ZRANGE',key,0,0,'WITHSCORES'); return {0,0,tonumber(oldest[2]),''} end; local expires=now+window; redis.call('ZADD',key,expires,leaseId); redis.call('PEXPIRE',key,window); return {1,capacity-count-1,expires,leaseId} end
        return redis.error_reply('Unsupported rate limit algorithm: '..algorithm)
        """;
    private const string ReleaseScript = "return redis.call('ZREM',KEYS[1],ARGV[1])";
    public async Task<RateLimitLease> AcquireAsync(string key, string algorithm, int permit, int seconds, int burst, CancellationToken token)
    {
        token.ThrowIfCancellationRequested(); if (!RateLimitAlgorithms.IsSupported(algorithm)) throw new ArgumentException($"Unsupported rate limit algorithm '{algorithm}'.");
        var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(); var requestId = Guid.NewGuid().ToString("N");
        var result = (RedisResult[]?)await redis.GetDatabase().ScriptEvaluateAsync(AcquireScript, [(RedisKey)key], [algorithm, permit, checked(seconds * 1000), burst, now, requestId]);
        if (result is null || result.Length != 4) throw new RedisException("The rate limit script returned an invalid result.");
        var id = result[3].ToString(); return new((long)result[0] == 1, key, algorithm, string.IsNullOrEmpty(id) ? null : id, checked(permit + burst), checked((int)(long)result[1]), (long)result[2] / 1000);
    }
    public async Task ReleaseAsync(RateLimitLease lease, CancellationToken token)
    { token.ThrowIfCancellationRequested(); if (lease.RequiresRelease) await redis.GetDatabase().ScriptEvaluateAsync(ReleaseScript, [(RedisKey)lease.Key], [lease.LeaseId!]); }
}
