using Business.Application.Abstractions.Persistence.Restaurant;
using Business.Application.Restaurant.Categories.Dtos;
using Dapper;

namespace Business.Infrastructure.Persistence;

public sealed class DapperCategoriesReadRepository(MySqlConnectionFactory connectionFactory)
    : ICategoryReadRepository
{
    private const string CategorySql = """
        SELECT c.Id, c.ParentId, c.Code, c.Name, c.Description, c.DisplayOrder,
               c.IsActive, c.UpdatedDate,
               EXISTS(SELECT 1 FROM restaurant_categories child WHERE child.ParentId = c.Id) HasChildren,
               (SELECT COUNT(*) FROM restaurant_foods f WHERE f.CategoryId = c.Id) DirectFoodCount
        FROM restaurant_categories c
        ORDER BY c.DisplayOrder, c.Name, c.Code
        """;

    public async Task<PagedCategoriesDto> GetAsync(
        string? search,
        CategoryStatusFilter status,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var rows = await LoadAsync(cancellationToken);
        var dtos = BuildTree(rows);
        var normalized = string.IsNullOrWhiteSpace(search) ? null : search.Trim();
        var matches = dtos.Where(item =>
            (normalized is null
                || item.Code.Contains(normalized, StringComparison.OrdinalIgnoreCase)
                || item.Name.Contains(normalized, StringComparison.OrdinalIgnoreCase))
            && MatchesStatus(item, status)).ToList();
        return new PagedCategoriesDto(
            matches.Skip((page - 1) * pageSize).Take(pageSize).ToList(),
            matches.Count,
            page,
            pageSize);
    }

    public async Task<CategoryDto?> GetByCodeAsync(
        string code,
        CancellationToken cancellationToken)
    {
        var dtos = BuildTree(await LoadAsync(cancellationToken));
        return dtos.FirstOrDefault(item =>
            string.Equals(item.Code, code, StringComparison.OrdinalIgnoreCase));
    }

    private async Task<List<CategoryRow>> LoadAsync(CancellationToken cancellationToken)
    {
        await using var connection = connectionFactory.CreateConnection();
        var rows = await connection.QueryAsync<CategoryRow>(new CommandDefinition(
            CategorySql,
            cancellationToken: cancellationToken));
        return rows.AsList();
    }

    private static List<CategoryDto> BuildTree(IReadOnlyList<CategoryRow> rows)
    {
        var byId = rows.ToDictionary(row => row.Id);
        var childCounts = rows
            .Where(row => row.ParentId.HasValue)
            .GroupBy(row => row.ParentId!.Value)
            .ToDictionary(group => group.Key, group => group.Count());

        (List<CategoryAncestorDto> Path, bool Effective) Resolve(CategoryRow row)
        {
            var path = new List<CategoryAncestorDto>();
            var effective = row.IsActive;
            var parentId = row.ParentId;
            var visited = new HashSet<ulong> { row.Id };
            while (parentId is ulong id && byId.TryGetValue(id, out var parent) && visited.Add(id))
            {
                path.Add(new CategoryAncestorDto(parent.Id, parent.Code, parent.Name));
                effective &= parent.IsActive;
                parentId = parent.ParentId;
            }
            path.Reverse();
            return (path, effective);
        }

        int Descendants(ulong id) => rows.Count(candidate =>
        {
            var parentId = candidate.ParentId;
            var visited = new HashSet<ulong>();
            while (parentId is ulong parent && byId.TryGetValue(parent, out var node) && visited.Add(parent))
            {
                if (parent == id) return true;
                parentId = node.ParentId;
            }
            return false;
        });

        return rows.Select(row =>
        {
            var resolved = Resolve(row);
            return new CategoryDto(
                row.Id,
                row.ParentId,
                row.Code,
                row.Name,
                row.Description,
                row.DisplayOrder,
                row.IsActive,
                resolved.Effective,
                resolved.Path.Count,
                resolved.Path,
                childCounts.ContainsKey(row.Id),
                Descendants(row.Id),
                row.DirectFoodCount,
                row.UpdatedDate);
        }).OrderBy(item => string.Join("/", item.AncestorPath.Select(x => x.Name).Append(item.Name)))
            .ToList();
    }

    private static bool MatchesStatus(CategoryDto item, CategoryStatusFilter status) => status switch
    {
        CategoryStatusFilter.Active => item.IsActive,
        CategoryStatusFilter.Inactive => !item.IsActive,
        CategoryStatusFilter.EffectiveActive => item.IsEffectivelyActive,
        CategoryStatusFilter.EffectiveInactive => !item.IsEffectivelyActive,
        _ => true
    };

    private sealed class CategoryRow
    {
        public ulong Id { get; init; }
        public ulong? ParentId { get; init; }
        public required string Code { get; init; }
        public required string Name { get; init; }
        public string? Description { get; init; }
        public int DisplayOrder { get; init; }
        public bool IsActive { get; init; }
        public DateTime UpdatedDate { get; init; }
        public bool HasChildren { get; init; }
        public int DirectFoodCount { get; init; }
    }
}
