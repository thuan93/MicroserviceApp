using Microsoft.Extensions.Hosting;
using Serilog;

namespace Common.Logging;

public static class Serilogger
{

    public static Action<HostBuilderContext, LoggerConfiguration> ConfigureLogger => (context, config) =>
    {
        var applicationName = context.HostingEnvironment.ApplicationName;
        var environmentName = context.HostingEnvironment.EnvironmentName;
        config
            .MinimumLevel.Debug()
            .WriteTo.Console(outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level}] {SourceContext}{Message:lj}{NewLine}{Exception}")
            .WriteTo.Debug(outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level}] {SourceContext}{Message:lj}{NewLine}{Exception}")
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithProperty("Environment", environmentName)
            .Enrich.WithProperty("Application", applicationName)
            .ReadFrom.Configuration(context.Configuration);
    };
}