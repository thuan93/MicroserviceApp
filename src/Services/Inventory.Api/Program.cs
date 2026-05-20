using AspNetCore.Extensions;
using Common.Logging;
using HealthChecks.UI.Client;
using Inventory.Api.Extensions;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddMicroserviceCors();
builder.Services.AddMicroserviceJwtAuthentication(builder.Configuration);
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddSwaggerServices(builder.Configuration);

builder.AddMicroserviceTelemetry("Inventory.Api");

builder.Host.UseSerilog(Serilogger.ConfigureLogger);

var app = builder.Build();

app.UseSerilogRequestLogging();

app.UseGlobalExceptionHandler();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Inventory.Api v1");
    c.RoutePrefix = "swagger";
});

app.UseMicroserviceCors();
app.UseMicroserviceJwtAuthentication(app.Configuration);
app.MapControllers();

app.MapHealthChecks("/health", new HealthCheckOptions
{
    Predicate = _ => true,
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
}).AllowAnonymous();

Log.Information("Starting Inventory.Api Service");

app.Run();
