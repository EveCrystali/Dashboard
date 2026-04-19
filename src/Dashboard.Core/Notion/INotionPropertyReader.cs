using System.Text.Json;
using Dashboard.Core.Domain;

namespace Dashboard.Core.Notion;

public interface INotionPropertyReader
{
    string? AsTitle(JsonElement property);
    string? AsRichText(JsonElement property);
    DateRange? AsDate(JsonElement property);
    string? AsSelect(JsonElement property);
    string? AsStatus(JsonElement property);
    IReadOnlyList<string> AsMultiSelect(JsonElement property);
    double? AsNumber(JsonElement property);
    bool AsCheckbox(JsonElement property);
    IReadOnlyList<string> AsRelation(JsonElement property);
    IReadOnlyList<string> AsPeople(JsonElement property);
    string? AsUrl(JsonElement property);
    IReadOnlyList<string> AsFiles(JsonElement property);
}
