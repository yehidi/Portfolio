using Microsoft.Extensions.Logging;

namespace Apigee.Client.Internal;

/// <summary>Compile-time source-generated log messages for the library (allocation-free when disabled).</summary>
internal static partial class ApigeeLog
{
    [LoggerMessage(EventId = 1, Level = LogLevel.Debug, Message = "Requesting a new Apigee access token from {TokenEndpoint}.")]
    public static partial void RequestingAccessToken(ILogger logger, string tokenEndpoint);

    [LoggerMessage(EventId = 2, Level = LogLevel.Information, Message = "Apigee access token acquired; valid until {ExpiresAtUtc:O}.")]
    public static partial void AccessTokenAcquired(ILogger logger, DateTimeOffset expiresAtUtc);

    [LoggerMessage(EventId = 3, Level = LogLevel.Error, Message = "Apigee token request failed with status code {StatusCode}.")]
    public static partial void TokenRequestFailed(ILogger logger, int statusCode);

    [LoggerMessage(EventId = 4, Level = LogLevel.Warning, Message = "Apigee request to {RequestUri} failed with status code {StatusCode}.")]
    public static partial void RequestFailed(ILogger logger, Uri? requestUri, int statusCode);
}
