using System.Globalization;
using System.Text.Json;
using Dashboard.Core.Domain;

namespace Dashboard.Core.Notion;

public sealed class NotionPropertyReader : INotionPropertyReader
{
    public string? AsTitle(JsonElement property) => JoinRichTextPlain(property, "title");

    public string? AsRichText(JsonElement property) => JoinRichTextPlain(property, "rich_text");

    public DateRange? AsDate(JsonElement property)
    {
        if (!TryGetTypedObject(property, "date", out var date) || date.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        var startText = ReadStringOrNull(date, "start");
        var endText = ReadStringOrNull(date, "end");
        if (startText is null && endText is null)
        {
            return null;
        }

        var start = ParseIsoDate(startText, out var startIsDateTime);
        var end = ParseIsoDate(endText, out var endIsDateTime);
        return new DateRange(start, end, startIsDateTime || endIsDateTime);
    }

    public string? AsSelect(JsonElement property) => ReadNamedOption(property, "select");

    public string? AsStatus(JsonElement property) => ReadNamedOption(property, "status");

    public IReadOnlyList<string> AsMultiSelect(JsonElement property)
    {
        if (!TryGetTypedObject(property, "multi_select", out var arr) || arr.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        var names = new List<string>();
        foreach (var item in arr.EnumerateArray())
        {
            if (item.TryGetProperty("name", out var name) && name.ValueKind == JsonValueKind.String)
            {
                names.Add(name.GetString()!);
            }
        }

        return names;
    }

    public double? AsNumber(JsonElement property)
    {
        if (!TryGetTypedObject(property, "number", out var n))
        {
            return null;
        }

        return n.ValueKind switch
        {
            JsonValueKind.Number => n.GetDouble(),
            _ => null,
        };
    }

    public bool AsCheckbox(JsonElement property)
    {
        if (!TryGetTypedObject(property, "checkbox", out var c))
        {
            return false;
        }

        return c.ValueKind == JsonValueKind.True;
    }

    public IReadOnlyList<string> AsRelation(JsonElement property) => ReadIdArray(property, "relation");

    public IReadOnlyList<string> AsPeople(JsonElement property) => ReadIdArray(property, "people");

    public string? AsUrl(JsonElement property)
    {
        if (!TryGetTypedObject(property, "url", out var u))
        {
            return null;
        }

        return u.ValueKind == JsonValueKind.String ? u.GetString() : null;
    }

    public IReadOnlyList<string> AsFiles(JsonElement property)
    {
        if (!TryGetTypedObject(property, "files", out var arr) || arr.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        var names = new List<string>();
        foreach (var item in arr.EnumerateArray())
        {
            if (item.TryGetProperty("name", out var n) && n.ValueKind == JsonValueKind.String)
            {
                names.Add(n.GetString()!);
            }
        }

        return names;
    }

    private static string? JoinRichTextPlain(JsonElement property, string typeKey)
    {
        if (!TryGetTypedObject(property, typeKey, out var arr) || arr.ValueKind != JsonValueKind.Array)
        {
            return null;
        }

        var parts = new List<string>();
        foreach (var item in arr.EnumerateArray())
        {
            if (item.TryGetProperty("plain_text", out var plain) && plain.ValueKind == JsonValueKind.String)
            {
                parts.Add(plain.GetString()!);
            }
        }

        if (parts.Count == 0)
        {
            return null;
        }

        return string.Concat(parts);
    }

    private static string? ReadNamedOption(JsonElement property, string typeKey)
    {
        if (!TryGetTypedObject(property, typeKey, out var sel) || sel.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        return sel.TryGetProperty("name", out var name) && name.ValueKind == JsonValueKind.String
            ? name.GetString()
            : null;
    }

    private static IReadOnlyList<string> ReadIdArray(JsonElement property, string typeKey)
    {
        if (!TryGetTypedObject(property, typeKey, out var arr) || arr.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        var ids = new List<string>();
        foreach (var item in arr.EnumerateArray())
        {
            if (item.TryGetProperty("id", out var id) && id.ValueKind == JsonValueKind.String)
            {
                ids.Add(id.GetString()!);
            }
        }

        return ids;
    }

    private static bool TryGetTypedObject(JsonElement property, string typeKey, out JsonElement value)
    {
        value = default;
        if (property.ValueKind != JsonValueKind.Object)
        {
            return false;
        }

        if (!property.TryGetProperty(typeKey, out var found) || found.ValueKind == JsonValueKind.Null)
        {
            return false;
        }

        value = found;
        return true;
    }

    private static string? ReadStringOrNull(JsonElement obj, string key) =>
        obj.TryGetProperty(key, out var v) && v.ValueKind == JsonValueKind.String ? v.GetString() : null;

    private static DateTimeOffset? ParseIsoDate(string? text, out bool isDateTime)
    {
        isDateTime = false;
        if (string.IsNullOrEmpty(text))
        {
            return null;
        }

        if (text.Contains('T', StringComparison.Ordinal))
        {
            isDateTime = true;
            return DateTimeOffset.Parse(text, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);
        }

        var date = DateTime.ParseExact(text, "yyyy-MM-dd", CultureInfo.InvariantCulture);
        return new DateTimeOffset(date, TimeSpan.Zero);
    }
}
