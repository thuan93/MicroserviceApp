using AspNetCore.Extensions;
using Microsoft.AspNetCore.RateLimiting;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("ocelot.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"ocelot.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true);

builder.Services.AddMicroserviceCors();
builder.Services.AddMicroserviceJwtAuthentication(builder.Configuration);
builder.Services.AddOcelot(builder.Configuration);

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = 429;
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 1,
                Window = TimeSpan.FromSeconds(1)
            }));
});

builder.AddMicroserviceTelemetry("OcelotApiGw");

var app = builder.Build();

app.UseGlobalExceptionHandler();
app.UseRateLimiter();
app.UseMicroserviceCors();
app.UseMicroserviceJwtAuthentication(app.Configuration);
await app.UseOcelot();

app.Run();
