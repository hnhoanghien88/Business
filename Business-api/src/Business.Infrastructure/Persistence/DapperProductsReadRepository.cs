using Business.Application.Abstractions.Persistence.Restaurant;
using Business.Application.Restaurant.Products.Dtos;
using Dapper;

namespace Business.Infrastructure.Persistence;

public sealed class DapperProductsReadRepository(MySqlConnectionFactory connectionFactory) : IProductReadRepository
{
    public async Task<ProductDto?> GetByCodeAsync(string code, CancellationToken cancellationToken)
    {
        await using var connection = connectionFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<ProductDto>(new CommandDefinition(
            "SELECT Id, CategoryId, Code, Name, IsActive FROM restaurant_foods WHERE Code = @Code AND IsActive = TRUE", new { Code = code }, cancellationToken: cancellationToken));
    }

    public async Task<PagedProductsDto> GetAsync(string? search, int page, int pageSize, CancellationToken cancellationToken)
    {
        const string where = " WHERE IsActive = TRUE AND (@Search IS NULL OR Code LIKE CONCAT('%', @Search, '%') OR Name LIKE CONCAT('%', @Search, '%'))";
        var parameters = new { Search = string.IsNullOrWhiteSpace(search) ? null : search, PageSize = pageSize, Offset = (page - 1) * pageSize };
        await using var connection = connectionFactory.CreateConnection();
        var items = await connection.QueryAsync<ProductDto>(new CommandDefinition(
            $"SELECT Id, CategoryId, Code, Name, IsActive FROM restaurant_foods{where} ORDER BY Code LIMIT @PageSize OFFSET @Offset", parameters, cancellationToken: cancellationToken));
        var totalCount = await connection.ExecuteScalarAsync<int>(new CommandDefinition(
            $"SELECT COUNT(*) FROM restaurant_foods{where}", parameters, cancellationToken: cancellationToken));
        return new PagedProductsDto(items.AsList(), totalCount, page, pageSize);
    }
}
