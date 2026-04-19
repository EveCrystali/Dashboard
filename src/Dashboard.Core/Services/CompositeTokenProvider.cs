using Dashboard.Core.Abstractions;
using Microsoft.Extensions.Configuration;

namespace Dashboard.Core.Services;

/// <summary>
/// D\u00e9corateur qui, si le provider primaire ne retourne rien, tombe en fallback sur
/// <see cref="IConfiguration"/> (user-secrets en Debug). Les \u00e9critures et la purge
/// sont toujours d\u00e9l\u00e9gu\u00e9es au provider primaire.
/// </summary>
public sealed class CompositeTokenProvider : ITokenProvider
{
    internal const string NotionTokenConfigurationKey = "Notion:IntegrationToken";

    private readonly ITokenProvider _primary;
    private readonly IConfiguration _configuration;

    public CompositeTokenProvider(ITokenProvider primary, IConfiguration configuration)
    {
        _primary = primary;
        _configuration = configuration;
    }

    public async Task<string?> GetNotionTokenAsync(CancellationToken ct = default)
    {
        var token = await _primary.GetNotionTokenAsync(ct).ConfigureAwait(false);
        if (!string.IsNullOrEmpty(token))
        {
            return token;
        }

        var fallback = _configuration[NotionTokenConfigurationKey];
        return string.IsNullOrEmpty(fallback) ? null : fallback;
    }

    public Task SetNotionTokenAsync(string token, CancellationToken ct = default)
        => _primary.SetNotionTokenAsync(token, ct);

    public Task ClearAsync(CancellationToken ct = default)
        => _primary.ClearAsync(ct);
}
