using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Sinks.Elasticsearch;

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

        var elasticsearchUrl = context.Configuration.GetValue<string>("Elasticsearch:Url");
        if (!string.IsNullOrEmpty(elasticsearchUrl))
        {
            config.WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(elasticsearchUrl))
            {
                AutoRegisterTemplate = true,
                IndexFormat = $"{applicationName.ToLower().Replace(".", "-")}-{environmentName.ToLower()}-{DateTime.UtcNow:yyyy-MM-dd}"
            });
        }
    };
}