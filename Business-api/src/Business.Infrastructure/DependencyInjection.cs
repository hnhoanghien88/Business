using Business.Application.Abstractions.Persistence.Restaurant;
using Business.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Business.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("RestaurantDatabase")
            ?? throw new InvalidOperationException(
                "Connection string 'RestaurantDatabase' was not found.");

        services.AddDbContext<BusinessDbContext>(options =>
            options.UseMySQL(connectionString));
        services.AddSingleton(new MySqlConnectionFactory(connectionString));
        services.AddScoped<IProductRepository, MySqlProductsRepository>();
        services.AddScoped<IProductReadRepository, DapperProductsReadRepository>();
        services.AddScoped<ICategoryRepository, MySqlCategoriesRepository>();
        services.AddScoped<ICategoryReadRepository, DapperCategoriesReadRepository>();

        return services;
    }
}
