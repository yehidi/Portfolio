using Apigee.Client.Abstractions;
using Apigee.Client.Configuration;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace Apigee.Client.Tests;

public sealed class ApigeeServiceCollectionExtensionsTests
{
    [Fact]
    public void AddApigeeClient_RegistersResolvableClientAndTokenProvider()
    {
        var services = new ServiceCollection();
        services.AddSingleton<Microsoft.Extensions.Logging.ILoggerFactory>(NullLoggerFactory.Instance);

        services.AddApigeeClient(options =>
        {
            options.BaseUrl = "https://api.example.com";
            options.TokenEndpoint = "https://auth.example.com/oauth/token";
            options.ClientId = "client-id";
            options.ClientSecret = "client-secret";
        });

        using var provider = services.BuildServiceProvider();

        provider.GetRequiredService<IApigeeClient>().Should().NotBeNull();
        provider.GetRequiredService<IApigeeTokenProvider>().Should().NotBeNull();
    }

    [Fact]
    public void AddApigeeClient_MissingRequiredOptions_FailsFastOnStartupValidation()
    {
        var services = new ServiceCollection();
        services.AddSingleton<Microsoft.Extensions.Logging.ILoggerFactory>(NullLoggerFactory.Instance);

        services.AddApigeeClient(_ => { });

        using var provider = services.BuildServiceProvider();
        var act = () => provider.GetRequiredService<IOptions<ApigeeClientOptions>>().Value;

        act.Should().Throw<OptionsValidationException>();
    }

    [Fact]
    public void AddApigeeClient_InvalidBaseUrl_FailsValidation()
    {
        var services = new ServiceCollection();
        services.AddSingleton<Microsoft.Extensions.Logging.ILoggerFactory>(NullLoggerFactory.Instance);

        services.AddApigeeClient(options =>
        {
            options.BaseUrl = "not-a-url";
            options.TokenEndpoint = "https://auth.example.com/oauth/token";
            options.ClientId = "client-id";
            options.ClientSecret = "client-secret";
        });

        using var provider = services.BuildServiceProvider();
        var act = () => provider.GetRequiredService<IOptions<ApigeeClientOptions>>().Value;

        act.Should().Throw<OptionsValidationException>();
    }
}
