using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetCore.Extensions;

public static class CorsExtensions
{
    private const string PolicyName = "AllowAll";

    public static IServiceCollection AddMicroserviceCors(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy(PolicyName, builder =>
            {
                builder.AllowAnyOrigin()
                       .AllowAnyMethod()
                       .AllowAnyHeader();
            });
        });

        return services;
    }

    public static IApplicationBuilder UseMicroserviceCors(this IApplicationBuilder app)
    {
        return app.UseCors(PolicyName);
    }
}
