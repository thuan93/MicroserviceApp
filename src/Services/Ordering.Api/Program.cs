using AspNetCore.Extensions;
using Common.Logging;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Ordering.Api.Extensions;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddMicroserviceJwtAuthentication(builder.Configuration);
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddSwaggerServices(builder.Configuration);

builder.AddMicroserviceTelemetry("Ordering.Api");

builder.Host.UseSerilog(Serilogger.ConfigureLogger);

var app = builder.Build();

app.UseSerilogRequestLogging();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Ordering.Api v1");
    c.RoutePrefix = "swagger";
});

app.UseMicroserviceJwtAuthentication(app.Configuration);
app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks("/health", new HealthCheckOptions
{
    Predicate = _ => true,
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
}).AllowAnonymous();

Log.Information("Starting Ordering.Api Service");

app.Run();
