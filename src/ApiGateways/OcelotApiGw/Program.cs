using AspNetCore.Extensions;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("ocelot.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"ocelot.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true);

builder.Services.AddMicroserviceCors();
builder.Services.AddMicroserviceJwtAuthentication(builder.Configuration);
builder.Services.AddOcelot(builder.Configuration);

builder.AddMicroserviceTelemetry("OcelotApiGw");

var app = builder.Build();

app.UseGlobalExceptionHandler();
app.UseMicroserviceCors();
app.UseMicroserviceJwtAuthentication(app.Configuration);
await app.UseOcelot();

app.Run();
