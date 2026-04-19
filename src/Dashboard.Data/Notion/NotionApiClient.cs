using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Dashboard.Core.Notion;

namespace Dashboard.Data.Notion;

/// <summary>
/// Client bas niveau pour l'API Notion. Ne fait aucune interpr\u00e9tation m\u00e9tier :
/// le mapping des propri\u00e9t\u00e9s vers le domaine (<c>TodoItem</c>, <c>JobApplication</c>\u2026)
/// est de la responsabilit\u00e9 du Lot 5.
/// </summary>
public sealed class NotionApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _httpClient;

    public NotionApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <summary>
    /// Ex\u00e9cute une requ\u00eate <c>POST databases/{id}/query</c> et retourne un seul batch.
    /// </summary>
    public async Task<NotionQueryResponse> QueryDataSourceOneBatchAsync(
        string dataSourceId,
        string? startCursor,
        CancellationToken ct = default)
    {
        var body = startCursor is null
            ? new Dictionary<string, object?>()
            : new Dictionary<string, object?> { ["start_cursor"] = startCursor };

        using var response = await _httpClient
            .PostAsJsonAsync($"databases/{dataSourceId}/query", body, JsonOptions, ct)
            .ConfigureAwait(false);

        response.EnsureSuccessStatusCode();

        var payload = await response.Content
            .ReadFromJsonAsync<NotionQueryResponse>(JsonOptions, ct)
            .ConfigureAwait(false);

        return payload ?? new NotionQueryResponse();
    }

    /// <summary>
    /// It\u00e8re l'int\u00e9gralit\u00e9 des pages d'une data source en encha\u00eenant les batches
    /// jusqu'\u00e0 <see cref="NotionQueryResponse.HasMore"/> = <c>false</c>.
    /// </summary>
    public async IAsyncEnumerable<NotionPage> QueryDataSourceAsync(
        string dataSourceId,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        string? cursor = null;
        while (true)
        {
            var batch = await QueryDataSourceOneBatchAsync(dataSourceId, cursor, ct).ConfigureAwait(false);

            foreach (var page in batch.Results)
            {
                yield return page;
            }

            if (!batch.HasMore || string.IsNullOrEmpty(batch.NextCursor))
            {
                yield break;
            }

            cursor = batch.NextCursor;
        }
    }
}
