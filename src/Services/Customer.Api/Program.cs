using AspNetCore.Extensions;
using Common.Logging;
using Customer.Api.Extensions;
using FluentValidation.AspNetCore;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddMicroserviceCors();
builder.Services.AddMicroserviceJwtAuthentication(builder.Configuration);
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddSwaggerServices(builder.Configuration);

builder.AddMicroserviceTelemetry("Customer.Api");

builder.Host.UseSerilog(Serilogger.ConfigureLogger);

var app = builder.Build();

app.UseSerilogRequestLogging();

app.UseGlobalExceptionHandler();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Customer.Api v1");
    c.RoutePrefix = "swagger";
});

app.UseMicroserviceCors();
app.UseMicroserviceJwtAuthentication(app.Configuration);
app.UseHttpsRedirection();

app.MapControllers();

app.MapHealthChecks("/health", new HealthCheckOptions
{
    Predicate = _ => true,
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
}).AllowAnonymous();

Log.Information("Starting Customer.Api Service");

app.Run();
