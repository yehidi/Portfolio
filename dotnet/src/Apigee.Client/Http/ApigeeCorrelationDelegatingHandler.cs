using System.Diagnostics;
using Apigee.Client.Internal;

namespace Apigee.Client.Http;

/// <summary>
/// Ensures every outgoing request carries a correlation id, reusing the current
/// <see cref="Activity"/> id when one is available so traces line up across services.
/// </summary>
internal sealed class ApigeeCorrelationDelegatingHandler : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (!request.Headers.Contains(ApigeeDefaults.CorrelationHeaderName))
        {
            var correlationId = Activity.Current?.Id ?? Guid.NewGuid().ToString("N");
            request.Headers.TryAddWithoutValidation(ApigeeDefaults.CorrelationHeaderName, correlationId);
        }

        return base.SendAsync(request, cancellationToken);
    }
}
