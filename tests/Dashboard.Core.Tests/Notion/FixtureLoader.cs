using System.Text.Json;
using Dashboard.Core.Notion;

namespace Dashboard.Core.Tests.Notion;

internal static class FixtureLoader
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static NotionPage LoadPage(string fileName)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Notion", "Fixtures", fileName);
        using var stream = File.OpenRead(path);
        var page = JsonSerializer.Deserialize<NotionPage>(stream, JsonOptions);
        return page ?? throw new InvalidOperationException($"Fixture {fileName} vide.");
    }
}
