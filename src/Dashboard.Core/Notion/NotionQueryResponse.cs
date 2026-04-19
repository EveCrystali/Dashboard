using System.Text.Json.Serialization;

namespace Dashboard.Core.Notion;

/// <summary>
/// R\u00e9ponse d'un <c>POST /v1/databases/{id}/query</c>.
/// </summary>
public sealed class NotionQueryResponse
{
    [JsonPropertyName("results")]
    public IReadOnlyList<NotionPage> Results { get; set; } = [];

    [JsonPropertyName("next_cursor")]
    public string? NextCursor { get; set; }

    [JsonPropertyName("has_more")]
    public bool HasMore { get; set; }
}
