using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace AspNetCore.Extensions;

public static class JwtAuthenticationExtensions
{
    public const string SectionName = "Jwt";

    public static bool IsJwtConfigured(this IConfiguration configuration) =>
        !string.IsNullOrWhiteSpace(configuration[$"{SectionName}:Key"]);

    /// <summary>
    /// Registers JWT bearer validation when Jwt:Key is set.
    /// Adds a global authorization filter only when JWT is configured.
    /// </summary>
    public static IServiceCollection AddMicroserviceJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var key = configuration[$"{SectionName}:Key"];
        if (string.IsNullOrWhiteSpace(key))
            return services;

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = configuration[$"{SectionName}:Issuer"],
                    ValidAudience = configuration[$"{SectionName}:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
                };
            });

        services.AddAuthorization(options =>
        {
            options.FallbackPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();
        });

        return services;
    }

    public static IApplicationBuilder UseMicroserviceJwtAuthentication(this IApplicationBuilder app, IConfiguration configuration)
    {
        if (configuration.IsJwtConfigured())
        {
            app.UseAuthentication();
            app.UseAuthorization();
        }

        return app;
    }
}
