using Apigee.Client.Abstractions;
using Apigee.Client.Authentication;
using Apigee.Client.Configuration;
using Apigee.Client.Http;
using Apigee.Client.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Options;

namespace Apigee.Client;

/// <summary>Registers the Apigee client and all of its supporting services in an <see cref="IServiceCollection"/>.</summary>
public static class ApigeeServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="IApigeeClient"/> and <see cref="IApigeeTokenProvider"/>, binding
    /// <see cref="ApigeeClientOptions"/> from the given configuration section (defaults to "Apigee").
    /// </summary>
    public static IServiceCollection AddApigeeClient(
        this IServiceCollection services,
        IConfiguration configuration,
        string sectionName = ApigeeClientOptions.SectionName)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        return services.AddApigeeClient(configuration.GetSection(sectionName).Bind);
    }

    /// <summary>
    /// Registers <see cref="IApigeeClient"/> and <see cref="IApigeeTokenProvider"/>, configuring
    /// <see cref="ApigeeClientOptions"/> programmatically. Wires up OAuth2 token caching,
    /// correlation-id propagation, and a standard retry/circuit-breaker/timeout pipeline for
    /// both the token endpoint and the business API calls.
    /// </summary>
    public static IServiceCollection AddApigeeClient(
        this IServiceCollection services,
        Action<ApigeeClientOptions> configureOptions)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configureOptions);

        services.AddOptions<ApigeeClientOptions>()
            .Configure(configureOptions)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IValidateOptions<ApigeeClientOptions>, ApigeeClientOptionsValidator>());

        services.TryAddSingleton(TimeProvider.System);
        services.TryAddSingleton<IApigeeTokenProvider, ApigeeTokenProvider>();
        services.TryAddTransient<ApigeeAuthenticationDelegatingHandler>();
        services.TryAddTransient<ApigeeCorrelationDelegatingHandler>();

        // The resilience pipeline is built once, at startup, so its shape (retry attempts, etc.)
        // is derived from the options as configured right now rather than tracked live via IOptionsMonitor.
        var startupOptions = new ApigeeClientOptions();
        configureOptions(startupOptions);

        services.AddHttpClient(ApigeeDefaults.TokenHttpClientName, (sp, client) =>
            {
                var options = sp.GetRequiredService<IOptionsMonitor<ApigeeClientOptions>>().CurrentValue;
                client.Timeout = TimeSpan.FromSeconds(options.RequestTimeoutSeconds);
            })
            .AddStandardResilienceHandler(resilience => resilience.Retry.MaxRetryAttempts = startupOptions.RetryCount);

        services.AddHttpClient<IApigeeClient, ApigeeClient>(ApigeeDefaults.ApiHttpClientName, (sp, client) =>
            {
                var options = sp.GetRequiredService<IOptionsMonitor<ApigeeClientOptions>>().CurrentValue;
                client.BaseAddress = new Uri(options.BaseUrl, UriKind.Absolute);
                client.Timeout = TimeSpan.FromSeconds(options.RequestTimeoutSeconds);
            })
            // Handlers execute in the order they're added (outermost first); the correlation id
            // and bearer token must both be on the request before it reaches the resilience layer,
            // so that retries triggered inside that layer reuse the same headers.
            .AddHttpMessageHandler<ApigeeCorrelationDelegatingHandler>()
            .AddHttpMessageHandler<ApigeeAuthenticationDelegatingHandler>()
            .AddStandardResilienceHandler(resilience => resilience.Retry.MaxRetryAttempts = startupOptions.RetryCount);

        return services;
    }
}
