namespace Dashboard.Data.Notion;

/// <summary>
/// Options de configuration du client Notion, li\u00e9es \u00e0 la section <c>Notion</c>
/// de <see cref="Microsoft.Extensions.Configuration.IConfiguration"/>.
/// </summary>
public sealed class NotionOptions
{
    public const string SectionName = "Notion";

    /// <summary>URL de base de l'API Notion, avec <c>/</c> final.</summary>
    public string BaseAddress { get; set; } = "https://api.notion.com/v1/";

    /// <summary>Valeur de l'en-t\u00eate <c>Notion-Version</c>.</summary>
    public string NotionVersion { get; set; } = "2022-06-28";
}
