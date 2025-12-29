using MassTransit;
using Microsoft.EntityFrameworkCore;
using Ordering.Api.Consumers;
using Ordering.Api.Persistence;
using Ordering.Api.Repositories;
using Ordering.Api.Repositories.Interfaces;

namespace Ordering.Api.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Database - SQL Server
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        services.AddDbContext<OrderingContext>(options =>
            options.UseSqlServer(connectionString));

        // Repositories
        services.AddScoped<IOrderRepository, OrderRepository>();

        // MassTransit RabbitMQ
        services.AddMassTransit(config =>
        {
            // Register consumers
            config.AddConsumer<CustomerCreatedConsumer>();
            config.AddConsumer<CustomerUpdatedConsumer>();

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
            .AddSqlServer(connectionString!, name: "sqlserver", tags: new[] { "db", "sqlserver" });

        return services;
    }

    public static IServiceCollection AddSwaggerServices(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new() { Title = "Ordering.Api", Version = "v1" });
        });

        return services;
    }
}
