using Business.Infrastructure.Persistence.Migrations;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Migrations.Design;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Business.Api;

public sealed class DesignTimeServices : IDesignTimeServices
{
    public void ConfigureDesignTimeServices(IServiceCollection services)
    {
        services.Replace(ServiceDescriptor.Singleton<
            IMigrationsCodeGenerator,
            EmbeddedSqlMigrationsGenerator>());
    }
}
