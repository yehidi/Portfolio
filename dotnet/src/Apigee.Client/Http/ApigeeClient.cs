using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Apigee.Client.Abstractions;
using Apigee.Client.Exceptions;
using Apigee.Client.Internal;
using Microsoft.Extensions.Logging;

namespace Apigee.Client.Http;

/// <summary>Default <see cref="IApigeeClient"/> implementation built on a pre-configured, factory-managed <see cref="HttpClient"/>.</summary>
internal sealed class ApigeeClient : IApigeeClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _httpClient;
    private readonly ILogger<ApigeeClient> _logger;

    public ApigeeClient(HttpClient httpClient, ILogger<ApigeeClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public Task<TResponse?> GetAsync<TResponse>(string requestUri, CancellationToken cancellationToken = default) =>
        SendAsync<object?, TResponse>(HttpMethod.Get, requestUri, body: null, cancellationToken);

    public Task<TResponse?> PostAsync<TRequest, TResponse>(string requestUri, TRequest body, CancellationToken cancellationToken = default) =>
        SendAsync<TRequest, TResponse>(HttpMethod.Post, requestUri, body, cancellationToken);

    public Task<TResponse?> PutAsync<TRequest, TResponse>(string requestUri, TRequest body, CancellationToken cancellationToken = default) =>
        SendAsync<TRequest, TResponse>(HttpMethod.Put, requestUri, body, cancellationToken);

    public async Task DeleteAsync(string requestUri, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.DeleteAsync(requestUri, cancellationToken).ConfigureAwait(false);
        await EnsureSuccessAsync(response, cancellationToken).ConfigureAwait(false);
    }

    private async Task<TResponse?> SendAsync<TRequest, TResponse>(
        HttpMethod method, string requestUri, TRequest? body, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(method, requestUri);
        if (body is not null)
        {
            request.Content = JsonContent.Create(body, options: JsonOptions);
        }

        using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        await EnsureSuccessAsync(response, cancellationToken).ConfigureAwait(false);

        if (response.StatusCode == HttpStatusCode.NoContent)
        {
            return default;
        }

        return await response.Content.ReadFromJsonAsync<TResponse>(JsonOptions, cancellationToken).ConfigureAwait(false);
    }

    private async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        ApigeeLog.RequestFailed(_logger, response.RequestMessage?.RequestUri, (int)response.StatusCode);

        throw new ApigeeRequestException(
            $"Apigee request to '{response.RequestMessage?.RequestUri}' failed with status code {(int)response.StatusCode}.",
            (int)response.StatusCode,
            body);
    }
}
