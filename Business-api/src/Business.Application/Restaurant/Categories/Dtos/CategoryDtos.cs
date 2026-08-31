namespace Business.Application.Restaurant.Categories.Dtos;

public enum CategoryStatusFilter
{
    All,
    Active,
    Inactive,
    EffectiveActive,
    EffectiveInactive
}

public sealed record CategoryAncestorDto(ulong Id, string Code, string Name);

public sealed record CategoryDto(
    ulong Id,
    ulong? ParentId,
    string Code,
    string Name,
    string? Description,
    int DisplayOrder,
    bool IsActive,
    bool IsEffectivelyActive,
    int Depth,
    IReadOnlyList<CategoryAncestorDto> AncestorPath,
    bool HasChildren,
    int DescendantCount,
    int DirectFoodCount,
    DateTime Version);

public sealed record PagedCategoriesDto(
    IReadOnlyList<CategoryDto> Items,
    int TotalCount,
    int Page,
    int PageSize);
