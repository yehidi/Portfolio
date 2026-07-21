using System.Net.Http.Headers;
using Apigee.Client.Abstractions;

namespace Apigee.Client.Http;

/// <summary>Attaches a bearer access token, obtained from <see cref="IApigeeTokenProvider"/>, to every outgoing request.</summary>
internal sealed class ApigeeAuthenticationDelegatingHandler : DelegatingHandler
{
    private readonly IApigeeTokenProvider _tokenProvider;

    public ApigeeAuthenticationDelegatingHandler(IApigeeTokenProvider tokenProvider)
    {
        _tokenProvider = tokenProvider;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var accessToken = await _tokenProvider.GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
    }
}
