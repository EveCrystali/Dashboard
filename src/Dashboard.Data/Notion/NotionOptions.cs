namespace Dashboard.Data.Notion;

/// <summary>
/// Options de configuration du client Notion, liées à la section <c>Notion</c>
/// de <see cref="Microsoft.Extensions.Configuration.IConfiguration"/>.
/// </summary>
public sealed class NotionOptions
{
    public const string SectionName = "Notion";

    /// <summary>URL de base de l'API Notion, avec <c>/</c> final.</summary>
    public string BaseAddress { get; set; } = "https://api.notion.com/v1/";

    /// <summary>Valeur de l'en-tête <c>Notion-Version</c>.</summary>
    public string NotionVersion { get; set; } = "2022-06-28";

    /// <summary>UUID des data sources (bases Notion) consommées par l'application.</summary>
    public NotionDataSources DataSources { get; set; } = new();
}

public sealed class NotionDataSources
{
    public string Todos { get; set; } = "f5e0c493-1e75-4d8b-bcdd-f16f7c4c0ee5";
    public string JobApplications { get; set; } = "ddd8f621-4493-465a-80c4-657d461c04b0";
    public string Journal { get; set; } = "6a4776d3-f5f2-4dab-96de-98d66e5f3782";
    public string Health { get; set; } = "ff58ea49-daca-41da-9546-adbd93db18d6";
}
