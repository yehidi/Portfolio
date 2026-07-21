namespace Apigee.Client.Internal;

/// <summary>
/// Well-known names shared between the DI registration extensions and the internal
/// services that resolve <see cref="System.Net.Http.HttpClient"/> instances by name.
/// </summary>
internal static class ApigeeDefaults
{
    /// <summary>Named <see cref="System.Net.Http.HttpClient"/> used to call the Apigee token endpoint.</summary>
    public const string TokenHttpClientName = "Apigee.TokenClient";

    /// <summary>Named/typed <see cref="System.Net.Http.HttpClient"/> used for business API calls through Apigee.</summary>
    public const string ApiHttpClientName = "Apigee.ApiClient";

    /// <summary>Header used to propagate a correlation id across service boundaries.</summary>
    public const string CorrelationHeaderName = "X-Correlation-Id";
}
