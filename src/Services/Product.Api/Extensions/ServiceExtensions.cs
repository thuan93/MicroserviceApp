using Microsoft.EntityFrameworkCore;
using Product.Api.Persistence;
using Product.Api.Repositories;
using Product.Api.Repositories.Interfaces;

namespace Product.Api.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Database
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        services.AddDbContext<ProductContext>(options =>
            options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

        // Repositories
        services.AddScoped<IProductRepository, ProductRepository>();

        // Health Checks
        services.AddHealthChecks()
            .AddMySql(connectionString!, name: "mysql", tags: new[] { "db", "mysql" });

        return services;
    }

    public static IServiceCollection AddSwaggerServices(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new() { Title = "Product.Api", Version = "v1" });
        });

        return services;
    }
}
