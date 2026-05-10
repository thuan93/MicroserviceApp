using Microsoft.Extensions.DependencyInjection;

namespace AspNetCore.Extensions;

public static class HttpResilienceExtensions
{
    /// <summary>
    /// Standard resilience for outbound HTTP calls (retry, circuit breaker, timeout) — use with IHttpClientFactory.
    /// </summary>
    public static IHttpClientBuilder AddMicroserviceResilience(this IHttpClientBuilder builder)
    {
        builder.AddStandardResilienceHandler();
        return builder;
    }
}
