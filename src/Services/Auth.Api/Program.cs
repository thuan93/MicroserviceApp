using Common.Logging;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Host.UseSerilog(Serilogger.ConfigureLogger);

var app = builder.Build();

app.MapControllers();

Log.Information("Starting Auth.Api Service");

app.Run();
