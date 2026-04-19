namespace Dashboard.Core.Abstractions;

/// <summary>
/// Abstraction au-dessus du stockage s\u00e9curis\u00e9 plateforme (SecureStorage c\u00f4t\u00e9 MAUI).
/// Permet de tester les providers sans d\u00e9pendance MAUI.
/// </summary>
public interface ISecureStorageWrapper
{
    Task<string?> GetAsync(string key, CancellationToken ct = default);

    Task SetAsync(string key, string value, CancellationToken ct = default);

    bool Remove(string key);
}
