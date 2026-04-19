using System.Text.Json;
using Dashboard.Core.Domain;

namespace Dashboard.Core.Notion;

/// <summary>
/// Helpers de lecture sûre d'une propriété Notion à partir du dictionnaire
/// brut <see cref="NotionPage.Properties"/>. Toujours null-safe : si la clé
/// est absente, retourne la valeur par défaut (null / liste vide / false).
/// </summary>
public static class NotionPropertyExtensions
{
    public static string? ReadTitle(this IReadOnlyDictionary<string, JsonElement> properties, string name, INotionPropertyReader reader) =>
        properties.TryGetValue(name, out var el) ? reader.AsTitle(el) : null;

    public static string? ReadRichText(this IReadOnlyDictionary<string, JsonElement> properties, string name, INotionPropertyReader reader) =>
        properties.TryGetValue(name, out var el) ? reader.AsRichText(el) : null;

    public static DateRange? ReadDate(this IReadOnlyDictionary<string, JsonElement> properties, string name, INotionPropertyReader reader) =>
        properties.TryGetValue(name, out var el) ? reader.AsDate(el) : null;

    public static string? ReadSelect(this IReadOnlyDictionary<string, JsonElement> properties, string name, INotionPropertyReader reader) =>
        properties.TryGetValue(name, out var el) ? reader.AsSelect(el) : null;

    public static string? ReadStatus(this IReadOnlyDictionary<string, JsonElement> properties, string name, INotionPropertyReader reader) =>
        properties.TryGetValue(name, out var el) ? reader.AsStatus(el) : null;

    public static IReadOnlyList<string> ReadMultiSelect(this IReadOnlyDictionary<string, JsonElement> properties, string name, INotionPropertyReader reader) =>
        properties.TryGetValue(name, out var el) ? reader.AsMultiSelect(el) : [];

    public static double? ReadNumber(this IReadOnlyDictionary<string, JsonElement> properties, string name, INotionPropertyReader reader) =>
        properties.TryGetValue(name, out var el) ? reader.AsNumber(el) : null;

    public static bool ReadCheckbox(this IReadOnlyDictionary<string, JsonElement> properties, string name, INotionPropertyReader reader) =>
        properties.TryGetValue(name, out var el) && reader.AsCheckbox(el);

    public static IReadOnlyList<string> ReadRelation(this IReadOnlyDictionary<string, JsonElement> properties, string name, INotionPropertyReader reader) =>
        properties.TryGetValue(name, out var el) ? reader.AsRelation(el) : [];

    public static IReadOnlyList<string> ReadPeople(this IReadOnlyDictionary<string, JsonElement> properties, string name, INotionPropertyReader reader) =>
        properties.TryGetValue(name, out var el) ? reader.AsPeople(el) : [];

    public static string? ReadUrl(this IReadOnlyDictionary<string, JsonElement> properties, string name, INotionPropertyReader reader) =>
        properties.TryGetValue(name, out var el) ? reader.AsUrl(el) : null;

    public static IReadOnlyList<string> ReadFiles(this IReadOnlyDictionary<string, JsonElement> properties, string name, INotionPropertyReader reader) =>
        properties.TryGetValue(name, out var el) ? reader.AsFiles(el) : [];
}
