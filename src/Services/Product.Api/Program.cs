using AspNetCore.Extensions;
using Common.Logging;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Product.Api.Extensions;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddMicroserviceJwtAuthentication(builder.Configuration);
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddSwaggerServices(builder.Configuration);

builder.AddMicroserviceTelemetry("Product.Api");

builder.Host.UseSerilog(Serilogger.ConfigureLogger);

var app = builder.Build();

app.UseSerilogRequestLogging();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Product.Api v1");
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

Log.Information("Starting Product.Api Service");

app.Run();
