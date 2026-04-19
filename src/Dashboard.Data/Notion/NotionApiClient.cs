using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Dashboard.Core.Notion;

namespace Dashboard.Data.Notion;

/// <summary>
/// Client bas niveau pour l'API Notion. Ne fait aucune interprétation métier :
/// le mapping des propriétés vers le domaine (<c>TodoItem</c>, <c>JobApplication</c>…)
/// est de la responsabilité du Lot 5.
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
    /// Exécute une requête <c>POST databases/{id}/query</c> et retourne un seul batch.
    /// Le filtre <paramref name="editedOnOrAfter"/>, s'il est fourni, est traduit
    /// en <c>filter.last_edited_time.on_or_after</c> côté Notion (sync différentielle).
    /// </summary>
    public async Task<NotionQueryResponse> QueryDataSourceOneBatchAsync(
        string dataSourceId,
        string? startCursor,
        DateTimeOffset? editedOnOrAfter = null,
        CancellationToken ct = default)
    {
        var body = new Dictionary<string, object?>();
        if (startCursor is not null)
        {
            body["start_cursor"] = startCursor;
        }
        if (editedOnOrAfter is { } cutoff)
        {
            body["filter"] = new
            {
                timestamp = "last_edited_time",
                last_edited_time = new
                {
                    on_or_after = cutoff.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                },
            };
        }

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
    /// Itère l'intégralité des pages d'une data source en enchaînant les batches
    /// jusqu'à <see cref="NotionQueryResponse.HasMore"/> = <c>false</c>.
    /// </summary>
    public async IAsyncEnumerable<NotionPage> QueryDataSourceAsync(
        string dataSourceId,
        DateTimeOffset? editedOnOrAfter = null,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        string? cursor = null;
        while (true)
        {
            var batch = await QueryDataSourceOneBatchAsync(dataSourceId, cursor, editedOnOrAfter, ct).ConfigureAwait(false);

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
