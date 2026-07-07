namespace Apigee.Client.Exceptions;

/// <summary>Thrown when a call made through <see cref="Apigee.Client.Abstractions.IApigeeClient"/> receives a non-success HTTP response.</summary>
public sealed class ApigeeRequestException : ApigeeException
{
    /// <summary>Initializes a new instance for a non-success HTTP response from the downstream API.</summary>
    public ApigeeRequestException(string message, int statusCode, string? responseBody)
        : base(message)
    {
        StatusCode = statusCode;
        ResponseBody = responseBody;
    }

    /// <summary>HTTP status code returned by the downstream API.</summary>
    public int StatusCode { get; }

    /// <summary>Raw response body returned by the downstream API, when available.</summary>
    public string? ResponseBody { get; }
}
