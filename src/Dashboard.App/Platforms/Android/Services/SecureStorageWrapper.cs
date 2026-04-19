using Dashboard.Core.Abstractions;
using Microsoft.Maui.Storage;

namespace Dashboard.App.Platforms.Android.Services;

/// <summary>
/// Implémentation <see cref="ISecureStorageWrapper"/> adossée à <see cref="SecureStorage.Default"/>.
/// Le wrapper isole l'unique point de contact MAUI pour rendre les providers testables.
/// </summary>
internal sealed class SecureStorageWrapper : ISecureStorageWrapper
{
    public Task<string?> GetAsync(string key, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        return SecureStorage.Default.GetAsync(key);
    }

    public Task SetAsync(string key, string value, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        return SecureStorage.Default.SetAsync(key, value);
    }

    public bool Remove(string key) => SecureStorage.Default.Remove(key);
}
