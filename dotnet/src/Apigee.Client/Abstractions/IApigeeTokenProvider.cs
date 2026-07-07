namespace Apigee.Client.Abstractions;

/// <summary>
/// Supplies a valid OAuth2 access token for calling Apigee-fronted APIs, transparently
/// caching and refreshing it as needed. Implementations must be safe to call concurrently.
/// </summary>
public interface IApigeeTokenProvider
{
    /// <summary>
    /// Returns a cached access token if one is still valid, or acquires a new one otherwise.
    /// </summary>
    ValueTask<string> GetAccessTokenAsync(CancellationToken cancellationToken = default);
}
