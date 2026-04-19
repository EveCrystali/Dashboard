using Dashboard.Core.Domain;

namespace Dashboard.Core.Notion.Mappers;

/// <summary>
/// Mapping <see cref="NotionPage"/> → <see cref="JournalEntry"/> pour la base
/// « Journal Second Brain » (data source <c>6a4776d3-f5f2-4dab-96de-98d66e5f3782</c>).
/// </summary>
public static class JournalEntryMapper
{
    public const string ColumnTitle = "Titre";
    public const string ColumnDate = "Date";
    public const string ColumnType = "Type";
    public const string ColumnDomains = "Domaine";
    public const string ColumnSource = "Source";

    private static readonly IReadOnlyDictionary<string, JournalType> TypeByNotionName = new Dictionary<string, JournalType>(StringComparer.Ordinal)
    {
        ["Atome"] = JournalType.Atome,
        ["D\u00e9cision"] = JournalType.Decision,
        ["Snapshot"] = JournalType.Snapshot,
        ["Connexion"] = JournalType.Connexion,
    };

    private static readonly IReadOnlyDictionary<string, JournalDomain> DomainByNotionName = new Dictionary<string, JournalDomain>(StringComparer.Ordinal)
    {
        ["Emploi"] = JournalDomain.Emploi,
        ["Cassian"] = JournalDomain.Cassian,
        ["Sant\u00e9"] = JournalDomain.Sante,
        ["Infra"] = JournalDomain.Infra,
        ["Admin"] = JournalDomain.Admin,
        ["Transversal"] = JournalDomain.Transversal,
    };

    private static readonly IReadOnlyDictionary<string, JournalSource> SourceByNotionName = new Dictionary<string, JournalSource>(StringComparer.Ordinal)
    {
        ["Claude"] = JournalSource.Claude,
        ["Manuel"] = JournalSource.Manuel,
        ["Synchro"] = JournalSource.Synchro,
    };

    public static JournalEntry Map(NotionPage page, INotionPropertyReader reader)
    {
        ArgumentNullException.ThrowIfNull(page);
        ArgumentNullException.ThrowIfNull(reader);

        var p = page.Properties;
        var domains = p.ReadMultiSelect(ColumnDomains, reader);

        return new JournalEntry(
            Id: page.Id,
            Title: p.ReadTitle(ColumnTitle, reader) ?? string.Empty,
            Date: p.ReadDate(ColumnDate, reader),
            Type: LookupNullable(p.ReadSelect(ColumnType, reader), TypeByNotionName),
            Domains: LookupMulti(domains, DomainByNotionName),
            Source: LookupNullable(p.ReadSelect(ColumnSource, reader), SourceByNotionName),
            CreatedTime: page.CreatedTime);
    }

    private static TEnum? LookupNullable<TEnum>(string? name, IReadOnlyDictionary<string, TEnum> map)
        where TEnum : struct, Enum =>
        name is not null && map.TryGetValue(name, out var v) ? v : null;

    private static IReadOnlyList<TEnum> LookupMulti<TEnum>(IReadOnlyList<string> names, IReadOnlyDictionary<string, TEnum> map)
        where TEnum : struct, Enum
    {
        if (names.Count == 0)
        {
            return [];
        }

        var result = new List<TEnum>(names.Count);
        foreach (var n in names)
        {
            if (map.TryGetValue(n, out var v))
            {
                result.Add(v);
            }
        }

        return result;
    }
}
