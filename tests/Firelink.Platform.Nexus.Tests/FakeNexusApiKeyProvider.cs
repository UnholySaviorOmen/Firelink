using Firelink.Platform.Nexus;

namespace Firelink.Platform.Nexus.Tests;

/// <summary>
/// Fake-реализация INexusApiKeyProvider для тестов NexusClient.
/// </summary>
public sealed class FakeNexusApiKeyProvider : INexusApiKeyProvider
{
    public string? Key { get; set; }

    public FakeNexusApiKeyProvider(string? key = null)
    {
        Key = key;
    }

    public string? TryGetApiKey() => Key;
}
