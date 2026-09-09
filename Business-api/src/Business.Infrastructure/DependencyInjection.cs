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
        services.AddScoped<IFoodRepository, MySqlFoodsRepository>();
        services.AddScoped<IFoodReadRepository, DapperFoodsReadRepository>();
        services.AddScoped<ICategoryRepository, MySqlCategoriesRepository>();
        services.AddScoped<ICategoryReadRepository, DapperCategoriesReadRepository>();
        services.AddScoped<ILayoutRepository, MySqlLayoutsRepository>();
        services.AddScoped<ILayoutReadRepository, DapperLayoutsReadRepository>();
        services.AddScoped<ITableOperationsRepository, MySqlTableOperationsRepository>();
        services.AddScoped<ITableOperationsReadRepository, DapperTableOperationsReadRepository>();
        services.AddScoped<IOrderingRepository, MySqlOrderingRepository>();
        services.AddScoped<IOrderingReadRepository, DapperOrderingReadRepository>();
        services.AddScoped<IOrderingRepository, MySqlOrderingRepository>();
        services.AddScoped<IOrderingReadRepository, DapperOrderingReadRepository>();

        return services;
    }
}
