using System.Net.Http.Headers;
using Dashboard.Core.Abstractions;
using Microsoft.Extensions.Options;

namespace Dashboard.Data.Notion;

/// <summary>
/// <see cref="DelegatingHandler"/> qui, \u00e0 chaque requ\u00eate, injecte le token Notion
/// courant (lu via <see cref="ITokenProvider"/>) et le header <c>Notion-Version</c>.
/// Lire le token \u00e0 chaque appel permet de supporter une rotation transparente.
/// </summary>
public sealed class NotionAuthenticationHandler : DelegatingHandler
{
    private readonly ITokenProvider _tokenProvider;
    private readonly IOptions<NotionOptions> _options;

    public NotionAuthenticationHandler(ITokenProvider tokenProvider, IOptions<NotionOptions> options)
    {
        _tokenProvider = tokenProvider;
        _options = options;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var token = await _tokenProvider.GetNotionTokenAsync(cancellationToken).ConfigureAwait(false);
        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        if (!request.Headers.Contains("Notion-Version"))
        {
            request.Headers.Add("Notion-Version", _options.Value.NotionVersion);
        }

        return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
    }
}
