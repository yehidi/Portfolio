namespace Apigee.Client.Exceptions;

/// <summary>Thrown when Apigee rejects or fails to issue an OAuth2 access token.</summary>
public sealed class ApigeeAuthenticationException : ApigeeException
{
    /// <summary>Initializes a new instance for a non-success HTTP response from the token endpoint.</summary>
    public ApigeeAuthenticationException(string message, int? statusCode = null, string? responseBody = null)
        : base(message)
    {
        StatusCode = statusCode;
        ResponseBody = responseBody;
    }

    /// <summary>Initializes a new instance wrapping an unexpected error, such as a JSON parsing failure.</summary>
    public ApigeeAuthenticationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <summary>HTTP status code returned by the token endpoint, when available.</summary>
    public int? StatusCode { get; }

    /// <summary>Raw response body returned by the token endpoint, when available.</summary>
    public string? ResponseBody { get; }
}
