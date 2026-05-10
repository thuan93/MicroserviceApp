using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace AspNetCore.Extensions;

public static class OpenTelemetryExtensions
{
    /// <summary>
    /// Adds ASP.NET Core and HttpClient tracing with OTLP export (Jaeger, Aspire, etc.).
    /// Set OpenTelemetry:Enabled to false to disable. Endpoint defaults to OTEL_EXPORTER_OTLP_ENDPOINT or http://localhost:4317.
    /// </summary>
    public static WebApplicationBuilder AddMicroserviceTelemetry(this WebApplicationBuilder builder, string serviceName)
    {
        if (!builder.Configuration.GetValue("OpenTelemetry:Enabled", true))
            return builder;

        var endpoint =
            builder.Configuration["OpenTelemetry:OtlpEndpoint"]
            ?? Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT")
            ?? "http://localhost:4317";

        if (!Uri.TryCreate(endpoint, UriKind.Absolute, out var uri))
            return builder;

        builder.Services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(serviceName))
            .WithTracing(tracing => tracing
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddOtlpExporter(o => o.Endpoint = uri));

        return builder;
    }
}
