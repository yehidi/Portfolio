# Apigee.Client

A small, dependency-injection-friendly HTTP client for .NET 8 microservices that talk to
an internal or partner API through an [Apigee](https://cloud.google.com/apigee) gateway.

It takes care of the plumbing every service ends up rewriting by hand:

- **OAuth2 client-credentials token acquisition and caching**, with automatic refresh shortly
  before expiry and single-flight de-duplication of concurrent refreshes.
- **Correlation-id propagation** on every outgoing request.
- **Retry, circuit-breaker and timeout resilience** (via `Microsoft.Extensions.Http.Resilience`)
  for both the token endpoint and the business API calls.
- **Typed exceptions** (`ApigeeAuthenticationException`, `ApigeeRequestException`) carrying the
  HTTP status code and response body, instead of leaking raw `HttpRequestException`s.

## Install

```bash
dotnet add package Apigee.Client
```

## Configure

`appsettings.json`:

```json
{
  "Apigee": {
    "BaseUrl": "https://api.mycompany.com",
    "TokenEndpoint": "https://api.mycompany.com/oauth/token",
    "ClientId": "my-service-client-id",
    "ClientSecret": "my-service-client-secret",
    "Scope": "orders.read orders.write",
    "RequestTimeoutSeconds": 30,
    "TokenExpiryBufferSeconds": 60,
    "RetryCount": 3
  }
}
```

`ClientSecret` should come from a secret store (Azure Key Vault, AWS Secrets Manager, environment
variables, etc.) in any real deployment — keep it out of source-controlled `appsettings.json`.

`Program.cs`:

```csharp
builder.Services.AddApigeeClient(builder.Configuration);
```

Or configure it programmatically instead of binding from `IConfiguration`:

```csharp
builder.Services.AddApigeeClient(options =>
{
    options.BaseUrl = "https://api.mycompany.com";
    options.TokenEndpoint = "https://api.mycompany.com/oauth/token";
    options.ClientId = clientIdFromKeyVault;
    options.ClientSecret = clientSecretFromKeyVault;
});
```

Invalid configuration (missing client id, malformed URLs, etc.) fails fast at startup via
`ValidateOnStart()`, rather than surfacing as a confusing runtime error on the first request.

## Use

Inject `IApigeeClient` wherever you need to call the downstream API:

```csharp
public sealed class OrdersService(IApigeeClient apigeeClient)
{
    public Task<Order?> GetOrderAsync(string orderId, CancellationToken cancellationToken) =>
        apigeeClient.GetAsync<Order>($"/orders/{orderId}", cancellationToken);

    public Task<Order?> CreateOrderAsync(CreateOrderRequest request, CancellationToken cancellationToken) =>
        apigeeClient.PostAsync<CreateOrderRequest, Order>("/orders", request, cancellationToken);
}
```

Request paths are relative to `Apigee:BaseUrl`. Authentication, correlation ids and retries are
applied automatically — callers only deal with request/response models.

Handle failures with the typed exceptions:

```csharp
try
{
    var order = await apigeeClient.GetAsync<Order>($"/orders/{orderId}", cancellationToken);
}
catch (ApigeeRequestException ex) when (ex.StatusCode == 404)
{
    return NotFound();
}
```

If you only need the access token itself (e.g. to call a client not built on `HttpClient`),
inject `IApigeeTokenProvider` directly:

```csharp
public sealed class LegacySoapGateway(IApigeeTokenProvider tokenProvider)
{
    public async Task CallAsync(CancellationToken cancellationToken)
    {
        var accessToken = await tokenProvider.GetAccessTokenAsync(cancellationToken);
        // attach accessToken to whatever transport you're using
    }
}
```

## Design notes

- The token is cached as a single `AccessToken` per process (this library targets the
  client-credentials grant, which yields one token for the whole service — not per-user tokens).
- Both the token endpoint and business API calls run through
  [`Microsoft.Extensions.Http.Resilience`](https://learn.microsoft.com/dotnet/core/resilience/)'s
  standard pipeline (retry with jittered exponential backoff, circuit breaker, and a total-request
  timeout), so retries never repeat outer concerns like re-authenticating or re-generating the
  correlation id.
- `TimeProvider` is used instead of `DateTime.UtcNow` throughout, keeping token-expiry logic
  deterministically testable.
