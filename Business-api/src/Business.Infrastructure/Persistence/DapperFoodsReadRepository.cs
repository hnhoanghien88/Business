using Business.Application.Abstractions.Persistence.Restaurant;
using Business.Application.Restaurant.Foods.Dtos;
using Dapper;

namespace Business.Infrastructure.Persistence;

public sealed class DapperFoodsReadRepository(
    MySqlConnectionFactory connectionFactory) : IFoodReadRepository
{
    public async Task<IReadOnlyList<FoodPriceHistoryDto>> GetPriceHistoryAsync(
        string foodCode,
        string variantCode,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT h.Id, h.Price, h.EffectiveFrom, h.EffectiveTo, h.CreatedBy
            FROM restaurant_food_price_histories h
            INNER JOIN restaurant_food_variants v ON v.Id = h.FoodVariantId
            INNER JOIN restaurant_foods f ON f.Id = v.FoodId
            WHERE f.Code = @FoodCode AND v.Code = @VariantCode
            ORDER BY h.EffectiveFrom DESC;
            """;
        await using var connection = connectionFactory.CreateConnection();
        var rows = await connection.QueryAsync<FoodPriceHistoryDto>(
            new CommandDefinition(
                sql,
                new { FoodCode = foodCode, VariantCode = variantCode },
                cancellationToken: cancellationToken));
        return rows.AsList();
    }

    public async Task<FoodDto?> GetByCodeAsync(
        string code,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT f.Id, f.CategoryId, c.Code CategoryCode, c.Name CategoryName,
                   f.Code, f.Name, f.Description, f.ImageUrl, f.DisplayOrder,
                   f.IsActive, f.UpdatedDate Version
            FROM restaurant_foods f
            INNER JOIN restaurant_categories c ON c.Id = f.CategoryId
            WHERE f.Code = @Code;
            SELECT v.Id, v.Code, v.Name, v.CurrentPrice, v.IsDefault,
                   v.IsAvailable, v.SoldOutReason, v.DisplayOrder,
                   v.IsActive, v.UpdatedDate Version
            FROM restaurant_food_variants v
            INNER JOIN restaurant_foods f ON f.Id = v.FoodId
            WHERE f.Code = @Code
            ORDER BY v.DisplayOrder, v.Name, v.Code;
            """;
        await using var connection = connectionFactory.CreateConnection();
        using var result = await connection.QueryMultipleAsync(
            new CommandDefinition(sql, new { Code = code }, cancellationToken: cancellationToken));
        var header = await result.ReadSingleOrDefaultAsync<FoodHeader>();
        if (header is null)
            return null;
        var variants = (await result.ReadAsync<FoodVariantDto>()).AsList();
        return new FoodDto(
            header.Id,
            header.CategoryId,
            header.CategoryCode,
            header.CategoryName,
            header.Code,
            header.Name,
            header.Description,
            header.ImageUrl,
            header.DisplayOrder,
            header.IsActive,
            header.Version,
            variants);
    }

    public async Task<PagedFoodsDto> GetAsync(
        string? search,
        ulong? categoryId,
        FoodStatusFilter status,
        FoodAvailabilityFilter availability,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        const string fromWhere = """
            FROM restaurant_foods f
            INNER JOIN restaurant_categories c ON c.Id = f.CategoryId
            LEFT JOIN restaurant_food_variants v ON v.FoodId = f.Id
            WHERE (@Search IS NULL
                   OR f.Code LIKE CONCAT('%', @Search, '%')
                   OR f.Name LIKE CONCAT('%', @Search, '%'))
              AND (@CategoryId IS NULL OR f.CategoryId = @CategoryId)
              AND (@Status = 0 OR f.IsActive = (@Status = 1))
              AND (@Availability = 0 OR EXISTS (
                    SELECT 1 FROM restaurant_food_variants av
                    WHERE av.FoodId = f.Id AND av.IsActive = TRUE
                      AND av.IsAvailable = (@Availability = 1)))
            """;
        var parameters = new
        {
            Search = string.IsNullOrWhiteSpace(search) ? null : search,
            CategoryId = categoryId,
            Status = (int)status,
            Availability = (int)availability,
            PageSize = pageSize,
            Offset = (page - 1) * pageSize
        };
        var listSql = $"""
            SELECT f.Id, f.CategoryId, c.Code CategoryCode, c.Name CategoryName,
                   f.Code, f.Name, f.Description, f.ImageUrl, f.DisplayOrder,
                   f.IsActive,
                   COUNT(v.Id) VariantCount,
                   MIN(CASE WHEN v.IsActive THEN v.CurrentPrice END) MinimumPrice,
                   MAX(CASE WHEN v.IsActive THEN v.CurrentPrice END) MaximumPrice,
                   (f.IsActive AND c.IsActive AND
                    SUM(v.IsActive AND v.IsAvailable) > 0) IsEffectivelySellable,
                   f.UpdatedDate Version
            {fromWhere}
            GROUP BY f.Id, f.CategoryId, c.Code, c.Name, f.Code, f.Name,
                     f.Description, f.ImageUrl, f.DisplayOrder, f.IsActive, c.IsActive,
                     f.UpdatedDate
            ORDER BY f.DisplayOrder, f.Name, f.Code
            LIMIT @PageSize OFFSET @Offset;
            SELECT COUNT(DISTINCT f.Id) {fromWhere};
            """;
        await using var connection = connectionFactory.CreateConnection();
        using var result = await connection.QueryMultipleAsync(
            new CommandDefinition(listSql, parameters, cancellationToken: cancellationToken));
        var rows = (await result.ReadAsync<FoodSummaryRow>()).AsList();
        var items = rows.Select(row => new FoodSummaryDto(
            row.Id,
            row.CategoryId,
            row.CategoryCode,
            row.CategoryName,
            row.Code,
            row.Name,
            row.Description,
            row.ImageUrl,
            row.DisplayOrder,
            row.IsActive,
            checked((int)row.VariantCount),
            row.MinimumPrice,
            row.MaximumPrice,
            row.IsEffectivelySellable != 0,
            row.Version)).ToList();
        var total = await result.ReadSingleAsync<int>();
        return new PagedFoodsDto(items, total, page, pageSize);
    }

    private sealed record FoodSummaryRow(
        ulong Id,
        ulong CategoryId,
        string CategoryCode,
        string CategoryName,
        string Code,
        string Name,
        string? Description,
        string? ImageUrl,
        int DisplayOrder,
        bool IsActive,
        long VariantCount,
        decimal? MinimumPrice,
        decimal? MaximumPrice,
        long IsEffectivelySellable,
        DateTime Version);

    private sealed record FoodHeader(
        ulong Id,
        ulong CategoryId,
        string CategoryCode,
        string CategoryName,
        string Code,
        string Name,
        string? Description,
        string? ImageUrl,
        int DisplayOrder,
        bool IsActive,
        DateTime Version);
}
