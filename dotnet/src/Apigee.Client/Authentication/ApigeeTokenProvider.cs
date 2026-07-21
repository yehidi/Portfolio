using Apigee.Client.Abstractions;
using Apigee.Client.Configuration;
using Apigee.Client.Exceptions;
using Apigee.Client.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Apigee.Client.Authentication;

/// <summary>
/// Acquires and caches OAuth2 access tokens from Apigee using the client-credentials grant.
/// A single token is shared across the process; concurrent callers that observe an expired
/// token collapse into one outstanding refresh request ("single flight") instead of each
/// hammering the token endpoint.
/// </summary>
internal sealed class ApigeeTokenProvider : IApigeeTokenProvider, IDisposable
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IOptionsMonitor<ApigeeClientOptions> _options;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<ApigeeTokenProvider> _logger;
    private readonly SemaphoreSlim _refreshLock = new(1, 1);

    private volatile ApigeeAccessToken? _cachedToken;

    public ApigeeTokenProvider(
        IHttpClientFactory httpClientFactory,
        IOptionsMonitor<ApigeeClientOptions> options,
        TimeProvider timeProvider,
        ILogger<ApigeeTokenProvider> logger)
    {
        _httpClientFactory = httpClientFactory;
        _options = options;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    public async ValueTask<string> GetAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        var options = _options.CurrentValue;
        var buffer = TimeSpan.FromSeconds(options.TokenExpiryBufferSeconds);

        if (TryGetCachedToken(buffer, out var token))
        {
            return token;
        }

        await _refreshLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            // Another caller may have already refreshed the token while we waited for the lock.
            if (TryGetCachedToken(buffer, out token))
            {
                return token;
            }

            var refreshed = await RequestNewTokenAsync(options, cancellationToken).ConfigureAwait(false);
            _cachedToken = refreshed;
            return refreshed.Value;
        }
        finally
        {
            _refreshLock.Release();
        }
    }

    private bool TryGetCachedToken(TimeSpan buffer, out string value)
    {
        var token = _cachedToken;
        if (token is not null && !token.IsExpired(buffer, _timeProvider))
        {
            value = token.Value;
            return true;
        }

        value = string.Empty;
        return false;
    }

    private async Task<ApigeeAccessToken> RequestNewTokenAsync(ApigeeClientOptions options, CancellationToken cancellationToken)
    {
        ApigeeLog.RequestingAccessToken(_logger, options.TokenEndpoint);

        using var httpClient = _httpClientFactory.CreateClient(ApigeeDefaults.TokenHttpClientName);
        using var request = new HttpRequestMessage(HttpMethod.Post, options.TokenEndpoint)
        {
            Content = new FormUrlEncodedContent(BuildTokenRequestParameters(options)),
        };

        using var response = await httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        var payload = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            ApigeeLog.TokenRequestFailed(_logger, (int)response.StatusCode);
            throw new ApigeeAuthenticationException(
                $"Apigee token request to '{options.TokenEndpoint}' failed with status code {(int)response.StatusCode}.",
                (int)response.StatusCode,
                payload);
        }

        ApigeeTokenResponse? tokenResponse;
        try
        {
            tokenResponse = System.Text.Json.JsonSerializer.Deserialize(payload, ApigeeTokenJsonContext.Default.ApigeeTokenResponse);
        }
        catch (System.Text.Json.JsonException ex)
        {
            throw new ApigeeAuthenticationException("Apigee token response could not be parsed as JSON.", ex);
        }

        if (tokenResponse is null || string.IsNullOrWhiteSpace(tokenResponse.AccessToken))
        {
            throw new ApigeeAuthenticationException(
                "Apigee token response did not contain an access token.",
                (int)response.StatusCode,
                payload);
        }

        var expiresAtUtc = _timeProvider.GetUtcNow().AddSeconds(tokenResponse.ExpiresInSeconds);
        ApigeeLog.AccessTokenAcquired(_logger, expiresAtUtc);

        return new ApigeeAccessToken(tokenResponse.AccessToken, expiresAtUtc);
    }

    private static IEnumerable<KeyValuePair<string, string>> BuildTokenRequestParameters(ApigeeClientOptions options)
    {
        yield return new KeyValuePair<string, string>("grant_type", "client_credentials");
        yield return new KeyValuePair<string, string>("client_id", options.ClientId);
        yield return new KeyValuePair<string, string>("client_secret", options.ClientSecret);

        if (!string.IsNullOrWhiteSpace(options.Scope))
        {
            yield return new KeyValuePair<string, string>("scope", options.Scope);
        }
    }

    public void Dispose() => _refreshLock.Dispose();
}
