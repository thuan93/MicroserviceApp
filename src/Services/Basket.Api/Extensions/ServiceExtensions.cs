using AspNetCore.Extensions;
using Basket.Api.Repositories;
using Basket.Api.Repositories.Interfaces;
using FluentValidation;
using MassTransit;
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

        // MassTransit RabbitMQ
        services.AddMassTransit(config =>
        {
            config.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(configuration["RabbitMQ:Host"] ?? "localhost", "/", h =>
                {
                    h.Username(configuration["RabbitMQ:Username"] ?? "guest");
                    h.Password(configuration["RabbitMQ:Password"] ?? "guest");
                });
            });
        });

        // FluentValidation
        services.AddValidatorsFromAssembly(typeof(ServiceExtensions).Assembly);

        // Health Checks
        services.AddHealthChecks()
            .AddRedis(configuration.GetConnectionString("Redis")!, 
                name: "redis", tags: new[] { "cache", "redis" });

        return services;
    }

    public static IServiceCollection AddSwaggerServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new() { Title = "Basket.Api", Version = "v1" });
            c.AddJwtBearerSecurity(configuration);
        });

        return services;
    }
}
