using System.Net;
using System.Text;
using Apigee.Client.Authentication;
using Apigee.Client.Configuration;
using Apigee.Client.Exceptions;
using Apigee.Client.Tests.TestSupport;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Time.Testing;
using Xunit;

namespace Apigee.Client.Tests.Authentication;

public sealed class ApigeeTokenProviderTests
{
    private static readonly ApigeeClientOptions Options = new()
    {
        BaseUrl = "https://api.example.com",
        TokenEndpoint = "https://auth.example.com/oauth/token",
        ClientId = "client-id",
        ClientSecret = "client-secret",
        TokenExpiryBufferSeconds = 10,
    };

    [Fact]
    public async Task GetAccessTokenAsync_CachesTokenUntilItIsCloseToExpiry()
    {
        var requestCount = 0;
        var handler = new StubHttpMessageHandler(_ =>
        {
            requestCount++;
            return TokenResponse("token-1", expiresInSeconds: 3600);
        });
        var provider = CreateProvider(handler, out _);

        var first = await provider.GetAccessTokenAsync();
        var second = await provider.GetAccessTokenAsync();

        first.Should().Be("token-1");
        second.Should().Be("token-1");
        requestCount.Should().Be(1, "the second call should reuse the cached, still-valid token");
    }

    [Fact]
    public async Task GetAccessTokenAsync_RefreshesTokenOnceItIsWithinTheExpiryBuffer()
    {
        var responses = new Queue<string>(["token-1", "token-2"]);
        var handler = new StubHttpMessageHandler(_ => TokenResponse(responses.Dequeue(), expiresInSeconds: 60));
        var provider = CreateProvider(handler, out var timeProvider);

        var first = await provider.GetAccessTokenAsync();
        timeProvider.Advance(TimeSpan.FromSeconds(55)); // inside the 10s buffer of a 60s token
        var second = await provider.GetAccessTokenAsync();

        first.Should().Be("token-1");
        second.Should().Be("token-2");
    }

    [Fact]
    public async Task GetAccessTokenAsync_ConcurrentCallsWithNoCachedToken_IssueOnlyOneTokenRequest()
    {
        var requestCount = 0;
        var handler = new StubHttpMessageHandler(async (_, ct) =>
        {
            Interlocked.Increment(ref requestCount);
            await Task.Delay(50, ct); // widen the race window so concurrent callers actually overlap
            return await TokenResponse("token-1", expiresInSeconds: 3600);
        });
        var provider = CreateProvider(handler, out _);

        var results = await Task.WhenAll(Enumerable.Range(0, 10).Select(_ => provider.GetAccessTokenAsync().AsTask()));

        results.Should().AllBe("token-1");
        requestCount.Should().Be(1, "concurrent refreshes must collapse into a single outstanding request");
    }

    [Fact]
    public async Task GetAccessTokenAsync_TokenEndpointReturnsError_ThrowsApigeeAuthenticationException()
    {
        var handler = new StubHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.Unauthorized)
        {
            Content = new StringContent("{\"error\":\"invalid_client\"}", Encoding.UTF8, "application/json"),
        });
        var provider = CreateProvider(handler, out _);

        var act = async () => await provider.GetAccessTokenAsync();

        var exception = await act.Should().ThrowAsync<ApigeeAuthenticationException>();
        exception.Which.StatusCode.Should().Be(401);
    }

    private static ApigeeTokenProvider CreateProvider(HttpMessageHandler handler, out FakeTimeProvider timeProvider)
    {
        timeProvider = new FakeTimeProvider(DateTimeOffset.UtcNow);
        return new ApigeeTokenProvider(
            new StubHttpClientFactory(handler),
            new TestOptionsMonitor<ApigeeClientOptions>(Options),
            timeProvider,
            NullLogger<ApigeeTokenProvider>.Instance);
    }

    private static Task<HttpResponseMessage> TokenResponse(string accessToken, int expiresInSeconds)
    {
        var json = $$"""{"access_token":"{{accessToken}}","expires_in":{{expiresInSeconds}},"token_type":"Bearer"}""";
        return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json"),
        });
    }
}
