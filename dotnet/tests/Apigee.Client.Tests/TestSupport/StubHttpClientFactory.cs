namespace Apigee.Client.Tests.TestSupport;

/// <summary>An <see cref="IHttpClientFactory"/> that always returns a client backed by the same handler.</summary>
internal sealed class StubHttpClientFactory : IHttpClientFactory
{
    private readonly HttpMessageHandler _handler;

    public StubHttpClientFactory(HttpMessageHandler handler)
    {
        _handler = handler;
    }

    public HttpClient CreateClient(string name) => new(_handler, disposeHandler: false);
}
