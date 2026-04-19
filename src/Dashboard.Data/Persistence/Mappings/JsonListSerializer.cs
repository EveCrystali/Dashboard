using System.Text.Json;

namespace Dashboard.Data.Persistence.Mappings;

internal static class JsonListSerializer
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = false,
    };

    public static string Serialize<T>(IReadOnlyList<T> values) =>
        values.Count == 0 ? "[]" : JsonSerializer.Serialize(values, Options);

    public static IReadOnlyList<T> Deserialize<T>(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return [];
        }
        return JsonSerializer.Deserialize<List<T>>(json, Options) ?? [];
    }
}
