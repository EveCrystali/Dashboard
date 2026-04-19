using System.Text.Json;
using System.Text.Json.Serialization;

namespace Dashboard.Core.Notion;

/// <summary>
/// DTO d'une page Notion retourn\u00e9e par un <c>query</c>. Les champs structurels
/// sont typ\u00e9s ; les valeurs de propri\u00e9t\u00e9s, dont la forme d\u00e9pend du type Notion
/// (title, rich_text, date, number, select, status, checkbox\u2026), sont conserv\u00e9es
/// sous forme de <see cref="JsonElement"/> et interpr\u00e9t\u00e9es au Lot 5 (mapping domaine).
/// </summary>
public sealed class NotionPage
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("created_time")]
    public DateTimeOffset CreatedTime { get; set; }

    [JsonPropertyName("last_edited_time")]
    public DateTimeOffset LastEditedTime { get; set; }

    [JsonPropertyName("archived")]
    public bool Archived { get; set; }

    [JsonPropertyName("properties")]
    public IReadOnlyDictionary<string, JsonElement> Properties { get; set; } =
        new Dictionary<string, JsonElement>();
}
