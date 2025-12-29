using MassTransit;
using Microsoft.EntityFrameworkCore;
using Customer.Api.Persistence;
using Customer.Api.Repositories;
using Customer.Api.Repositories.Interfaces;

namespace Customer.Api.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Database - PostgreSQL
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        services.AddDbContext<CustomerContext>(options =>
            options.UseNpgsql(connectionString));

        // Repositories
        services.AddScoped<ICustomerRepository, CustomerRepository>();

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

        // Health Checks
        services.AddHealthChecks()
            .AddNpgSql(connectionString!, name: "postgresql", tags: new[] { "db", "postgresql" });

        return services;
    }

    public static IServiceCollection AddSwaggerServices(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new() { Title = "Customer.Api", Version = "v1" });
        });

        return services;
    }
}
