using Inventory.Api.Consumers;
using Inventory.Api.Repositories;
using Inventory.Api.Repositories.Interfaces;
using Inventory.Api.Settings;
using MassTransit;

namespace Inventory.Api.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        // MongoDB Settings
        services.Configure<MongoDbSettings>(configuration.GetSection("MongoDbSettings"));

        // Repositories
        services.AddScoped<IInventoryRepository, InventoryRepository>();

        // MassTransit RabbitMQ
        services.AddMassTransit(config =>
        {
            // Register consumers
            config.AddConsumer<ProductCreatedConsumer>();
            config.AddConsumer<ProductUpdatedConsumer>();
            config.AddConsumer<ProductStockUpdatedConsumer>();
            config.AddConsumer<ProductDeletedConsumer>();
            config.AddConsumer<OrderCreatedConsumer>();
            config.AddConsumer<OrderCancelledConsumer>();

            config.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(configuration["RabbitMQ:Host"] ?? "localhost", "/", h =>
                {
                    h.Username(configuration["RabbitMQ:Username"] ?? "guest");
                    h.Password(configuration["RabbitMQ:Password"] ?? "guest");
                });

                // Configure endpoints for consumers
                cfg.ConfigureEndpoints(context);
            });
        });

        // Health Checks
        services.AddHealthChecks()
            .AddMongoDb(configuration.GetSection("MongoDbSettings:ConnectionString").Value!, 
                name: "mongodb", tags: new[] { "db", "mongodb" });

        return services;
    }

    public static IServiceCollection AddSwaggerServices(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new() { Title = "Inventory.Api", Version = "v1" });
        });

        return services;
    }
}
