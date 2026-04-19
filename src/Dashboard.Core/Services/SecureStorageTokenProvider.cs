using Dashboard.Core.Abstractions;

namespace Dashboard.Core.Services;

/// <summary>
/// Provider qui persiste le token Notion dans le stockage s\u00e9curis\u00e9 plateforme.
/// </summary>
public sealed class SecureStorageTokenProvider : ITokenProvider
{
    internal const string NotionTokenKey = "notion.integration.token";

    private readonly ISecureStorageWrapper _storage;

    public SecureStorageTokenProvider(ISecureStorageWrapper storage)
    {
        _storage = storage;
    }

    public Task<string?> GetNotionTokenAsync(CancellationToken ct = default)
        => _storage.GetAsync(NotionTokenKey, ct);

    public Task SetNotionTokenAsync(string token, CancellationToken ct = default)
        => _storage.SetAsync(NotionTokenKey, token, ct);

    public Task ClearAsync(CancellationToken ct = default)
    {
        _storage.Remove(NotionTokenKey);
        return Task.CompletedTask;
    }
}
