namespace Apigee.Client.Abstractions;

/// <summary>
/// A minimal, JSON-oriented HTTP client for calling a business API behind Apigee.
/// Authentication, correlation-id propagation, retries and circuit breaking are handled
/// transparently by the pipeline this client is registered with; callers only deal with
/// request/response payloads.
/// </summary>
public interface IApigeeClient
{
    /// <summary>Sends a GET request and deserializes the JSON response body.</summary>
    Task<TResponse?> GetAsync<TResponse>(string requestUri, CancellationToken cancellationToken = default);

    /// <summary>Sends a POST request with a JSON body and deserializes the JSON response body.</summary>
    Task<TResponse?> PostAsync<TRequest, TResponse>(string requestUri, TRequest body, CancellationToken cancellationToken = default);

    /// <summary>Sends a PUT request with a JSON body and deserializes the JSON response body.</summary>
    Task<TResponse?> PutAsync<TRequest, TResponse>(string requestUri, TRequest body, CancellationToken cancellationToken = default);

    /// <summary>Sends a DELETE request.</summary>
    Task DeleteAsync(string requestUri, CancellationToken cancellationToken = default);
}
