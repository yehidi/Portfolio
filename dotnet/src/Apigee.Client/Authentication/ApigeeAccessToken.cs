namespace Apigee.Client.Authentication;

/// <summary>An access token together with the instant it stops being usable.</summary>
internal sealed record ApigeeAccessToken(string Value, DateTimeOffset ExpiresAtUtc)
{
    /// <summary>
    /// Whether the token should be treated as expired, taking <paramref name="buffer"/> into
    /// account so a refresh happens shortly before the real expiry.
    /// </summary>
    public bool IsExpired(TimeSpan buffer, TimeProvider timeProvider) =>
        timeProvider.GetUtcNow() + buffer >= ExpiresAtUtc;
}
