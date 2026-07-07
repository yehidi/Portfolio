using Microsoft.Extensions.Options;

namespace Apigee.Client.Tests.TestSupport;

/// <summary>A minimal <see cref="IOptionsMonitor{TOptions}"/> that always returns a fixed value; no change notifications.</summary>
internal sealed class TestOptionsMonitor<T>(T value) : IOptionsMonitor<T>
{
    public T CurrentValue { get; } = value;

    public T Get(string? name) => CurrentValue;

    public IDisposable? OnChange(Action<T, string?> listener) => null;
}
