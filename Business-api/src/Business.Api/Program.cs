using Business.Api.Middleware;
using Business.Application.Common.Behaviors;
using Business.Application.Restaurant.Products.CreateProduct;
using Business.Infrastructure;
using FluentValidation;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Json;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

var applicationLogPath = Path.Combine(builder.Environment.ContentRootPath, "logs", "application-.log");
var performanceLogPath = Path.Combine(builder.Environment.ContentRootPath, "logs", "performance-.log");
builder.Host.UseSerilog((context, _, configuration) =>
{
    configuration.MinimumLevel.Information().MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning).Enrich.FromLogContext()
        .WriteTo.Console(new JsonFormatter(renderMessage: true))
        .WriteTo.File(applicationLogPath, rollingInterval: RollingInterval.Day, rollOnFileSizeLimit: true, fileSizeLimitBytes: 25 * 1024 * 1024, retainedFileCountLimit: 30, shared: true, flushToDiskInterval: TimeSpan.FromSeconds(1))
        .WriteTo.Logger(performance => performance.Filter.ByIncludingOnly(logEvent => logEvent.Level >= LogEventLevel.Warning && logEvent.Properties.TryGetValue("SourceContext", out var source) && source.ToString().Contains("PerformanceBehavior", StringComparison.Ordinal))
            .WriteTo.File(performanceLogPath, rollingInterval: RollingInterval.Day, rollOnFileSizeLimit: true, fileSizeLimitBytes: 10 * 1024 * 1024, retainedFileCountLimit: 14, shared: true));
});

var performance = builder.Configuration.GetSection(PerformanceOptions.SectionName).Get<PerformanceOptions>() ?? new();
if (performance.SlowRequestThresholdMilliseconds <= 0) throw new InvalidOperationException("Observability:SlowRequestThresholdMilliseconds must be greater than zero.");
builder.Services.AddSingleton(performance);

builder.Services.AddControllers();
builder.Services.AddMemoryCache();
builder.Services.AddScoped<RateLimitPolicyProvider>();
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
    options.SwaggerDoc("v1", new() { Title = "Business API", Version = "v1" }));
builder.Services.AddMediatR(configuration =>
{
    configuration.RegisterServicesFromAssembly(typeof(CreateProductCommand).Assembly);
    configuration.AddOpenBehavior(typeof(PerformanceBehavior<,>));
    configuration.AddOpenBehavior(typeof(ValidationBehavior<,>));
});
builder.Services.AddTransient<IValidator<CreateProductCommand>, CreateProductValidator>();
builder.Services.AddTransient<IValidator<Business.Application.Restaurant.Products.UpdateProduct.UpdateProductCommand>, Business.Application.Restaurant.Products.UpdateProduct.UpdateProductValidator>();
var rateLimiting = builder.Configuration.GetSection(RateLimitingOptions.SectionName).Get<RateLimitingOptions>() ?? new();
if (rateLimiting.Store is not ("Redis" or "InMemory")) throw new InvalidOperationException("RateLimiting:Store must be either 'Redis' or 'InMemory'.");
if (rateLimiting.PolicyCacheSeconds <= 0) throw new InvalidOperationException("RateLimiting:PolicyCacheSeconds must be greater than zero.");
if (rateLimiting.FailureMode is not ("Open" or "Closed")) throw new InvalidOperationException("RateLimiting:FailureMode must be either 'Open' or 'Closed'.");
builder.Services.Configure<RateLimitingOptions>(builder.Configuration.GetSection(RateLimitingOptions.SectionName));
if (rateLimiting.Store == "Redis")
{
    var redis = builder.Configuration.GetConnectionString("Redis") ?? throw new InvalidOperationException("ConnectionStrings:Redis is required when RateLimiting:Store is Redis.");
    builder.Services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redis));
    builder.Services.AddSingleton<IRateLimitStore, RedisRateLimitStore>();
}
else builder.Services.AddSingleton<IRateLimitStore, InMemoryRateLimitStore>();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<StructuredRequestLoggingMiddleware>();
app.UseMiddleware<ExceptionMiddleware>();
app.UseHttpsRedirection();
app.UseRouting();
app.UseMiddleware<DynamicRateLimitMiddleware>();
app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "Healthy" }));

app.Run();
