using Basket.Api.Repositories;
using Basket.Api.Repositories.Interfaces;
using StackExchange.Redis;

namespace Basket.Api.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Redis
        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var redisConnection = configuration.GetConnectionString("Redis");
            return ConnectionMultiplexer.Connect(redisConnection!);
        });

        // Repositories
        services.AddScoped<IBasketRepository, BasketRepository>();

        // Health Checks
        services.AddHealthChecks()
            .AddRedis(configuration.GetConnectionString("Redis")!, 
                name: "redis", tags: new[] { "cache", "redis" });

        return services;
    }

    public static IServiceCollection AddSwaggerServices(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new() { Title = "Basket.Api", Version = "v1" });
        });

        return services;
    }
}
