# Apigee.Client — solution overview

A .NET 8 class library, packaged as a NuGet package, meant to be referenced by every
microservice that needs to call an API behind an [Apigee](https://cloud.google.com/apigee)
gateway. It centralizes OAuth2 authentication, correlation-id propagation and HTTP
resilience so individual services don't each reinvent it.

See [`src/Apigee.Client/README.md`](src/Apigee.Client/README.md) for the consumer-facing
usage guide (the same content that ships inside the `.nupkg`).

## Layout

```
dotnet/
├── Apigee.Client.sln
├── Directory.Build.props        # shared compiler/analyzer/package settings
├── .editorconfig
├── src/
│   └── Apigee.Client/
│       ├── ApigeeServiceCollectionExtensions.cs   # AddApigeeClient(...) — the public entry point
│       ├── Abstractions/         # IApigeeClient, IApigeeTokenProvider
│       ├── Configuration/        # ApigeeClientOptions + validation
│       ├── Authentication/       # OAuth2 token acquisition, caching, refresh
│       ├── Http/                 # DelegatingHandlers + the IApigeeClient implementation
│       ├── Exceptions/           # ApigeeException hierarchy
│       └── Internal/             # cross-cutting internals (defaults, source-generated logs)
└── tests/
    └── Apigee.Client.Tests/      # xUnit + Moq + FluentAssertions
```

### Why this shape

- **`Abstractions` is the public contract.** Everything a consumer touches — `IApigeeClient`,
  `IApigeeTokenProvider`, the options type, the exception types — is `public`. Every concrete
  implementation (`ApigeeClient`, `ApigeeTokenProvider`, the two `DelegatingHandler`s) is
  `internal`; consumers depend on interfaces, not on how token refresh or retries are implemented.
- **One DI entry point.** `ApigeeServiceCollectionExtensions.AddApigeeClient(...)` is the only
  thing a consuming service calls. It wires the whole pipeline — options validation, the token
  provider, both named `HttpClient`s, the delegating handlers in the right order, and the
  resilience handlers — so nothing can be registered half-correctly.
- **`TimeProvider` instead of `DateTime.UtcNow`.** Token-expiry logic is deterministically
  testable with `Microsoft.Extensions.TimeProvider.Testing`'s `FakeTimeProvider`, with no sleeps
  in the test suite.
- **`Microsoft.Extensions.Http.Resilience`** (Polly v8 under the hood) rather than hand-rolled
  retry loops — it's the framework-endorsed way to get retry/circuit-breaker/timeout behavior on
  an `HttpClient` in .NET 8.

## Build, test, pack

```bash
cd dotnet
dotnet build
dotnet test
dotnet pack src/Apigee.Client/Apigee.Client.csproj -c Release -o ./artifacts
```

`Directory.Build.props` enables nullable reference types, .NET analyzers, and
`TreatWarningsAsErrors` across every project in the solution, so `dotnet build` is the same gate
CI should run.

## Publishing a new version

1. Bump `VersionPrefix` in `Directory.Build.props` (or pass `-p:PackageVersion=x.y.z` to
   `dotnet pack`) following semantic versioning.
2. `dotnet pack src/Apigee.Client/Apigee.Client.csproj -c Release -o ./artifacts`.
3. `dotnet nuget push ./artifacts/Apigee.Client.<version>.nupkg -s <feed> -k <api-key>`.

In CI, wire this behind a release tag or a manual workflow dispatch rather than pushing a new
package on every commit to the default branch.

## Extending this package

This package intentionally does one thing: authenticated, resilient HTTP calls through Apigee.
If a specific microservice needs typed request/response models for a given downstream API, model
those in that service's own codebase (or a separate, API-specific package) built on top of
`IApigeeClient` — don't grow this package into a client for every individual API.
