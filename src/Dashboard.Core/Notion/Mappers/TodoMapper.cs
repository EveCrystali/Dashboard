using Dashboard.Core.Domain;

namespace Dashboard.Core.Notion.Mappers;

/// <summary>
/// Mapping <see cref="NotionPage"/> → <see cref="TodoItem"/> pour la base « Tâches »
/// (data source <c>f5e0c493-1e75-4d8b-bcdd-f16f7c4c0ee5</c>).
/// </summary>
/// <remarks>
/// Les apostrophes typographiques U+2019 (« ’ ») sont obligatoires pour
/// « Date d’échéance » et « Résumé généré par l’IA ».
/// </remarks>
public static class TodoMapper
{
    public const string ColumnTitle = "Nom";
    public const string ColumnStatus = "\u00c9tat";
    public const string ColumnPriority = "Priorit\u00e9";
    public const string ColumnTags = "Tag";
    public const string ColumnAgenda = "Agenda";
    public const string ColumnDueDate = "Date d\u2019\u00e9ch\u00e9ance";
    public const string ColumnAssignees = "Affectation";
    public const string ColumnAiSummary = "R\u00e9sum\u00e9 g\u00e9n\u00e9r\u00e9 par l\u2019IA";
    public const string ColumnSubtasks = "Sous-\u00e9l\u00e9ment";
    public const string ColumnParent = "\u00e9l\u00e9ment parent";

    private static readonly IReadOnlyDictionary<string, TodoStatus> StatusByNotionName = new Dictionary<string, TodoStatus>(StringComparer.Ordinal)
    {
        ["\ud83e\udde8 Du jour"] = TodoStatus.DuJour,
        ["\ud83d\udea6 T\u00e2che"] = TodoStatus.Tache,
        ["\ud83c\udfc7 En cours"] = TodoStatus.EnCours,
        ["\u26d4 Bloqu\u00e9e"] = TodoStatus.Bloquee,
        ["\u23f3 Z - En attente"] = TodoStatus.EnAttente,
        ["\ud83c\udfc1 Done"] = TodoStatus.Done,
        ["\u274c Annul\u00e9e"] = TodoStatus.Annulee,
    };

    private static readonly IReadOnlyDictionary<string, TodoPriority> PriorityByNotionName = new Dictionary<string, TodoPriority>(StringComparer.Ordinal)
    {
        ["\ud83d\udd25Haute \ud83d\udd25"] = TodoPriority.Haute,
        ["\u2728 Moyenne \u2728"] = TodoPriority.Moyenne,
        ["\u2744\ufe0f Basse"] = TodoPriority.Basse,
    };

    private static readonly IReadOnlyDictionary<string, TodoTag> TagByNotionName = new Dictionary<string, TodoTag>(StringComparer.Ordinal)
    {
        ["\ud83d\udc68\u200d\ud83d\udc69\u200d\ud83d\udc67\u200d\ud83d\udc66 Parentalit\u00e9"] = TodoTag.Parentalite,
        ["\u26bd Perso"] = TodoTag.Perso,
        ["\ud83d\udc76\ud83c\udffbCassian"] = TodoTag.Cassian,
        ["\ud83d\uddc4 Administratif"] = TodoTag.Administratif,
        ["\ud83c\udfe2 Logement"] = TodoTag.Logement,
        ["\ud83c\udfe6 AJEI"] = TodoTag.AJEI,
        ["\u2712 Travail"] = TodoTag.Travail,
        ["\ud83c\udf81 Cadeaux"] = TodoTag.Cadeaux,
        ["\ud83c\udf0d Voyage"] = TodoTag.Voyage,
        ["\ud83d\udc70 Mariage"] = TodoTag.Mariage,
        ["\ud83d\udd0e Recherche emploi"] = TodoTag.RechercheEmploi,
        ["\ud83d\udca1 Apprentissage"] = TodoTag.Apprentissage,
        ["\u271d\ufe0f Souvenir Fran\u00e7ais"] = TodoTag.SouvenirFrancais,
        ["\ud83d\udcbb Openclassroom"] = TodoTag.Openclassroom,
    };

    public static TodoItem Map(NotionPage page, INotionPropertyReader reader)
    {
        ArgumentNullException.ThrowIfNull(page);
        ArgumentNullException.ThrowIfNull(reader);

        var p = page.Properties;
        var parents = p.ReadRelation(ColumnParent, reader);

        return new TodoItem(
            Id: page.Id,
            Title: p.ReadTitle(ColumnTitle, reader) ?? string.Empty,
            Status: MapStatus(p.ReadStatus(ColumnStatus, reader)),
            Priority: MapPriority(p.ReadSelect(ColumnPriority, reader)),
            Tags: MapTags(p.ReadMultiSelect(ColumnTags, reader)),
            Agenda: p.ReadDate(ColumnAgenda, reader),
            DueDate: p.ReadDate(ColumnDueDate, reader),
            AssigneeIds: p.ReadPeople(ColumnAssignees, reader),
            AiSummary: p.ReadRichText(ColumnAiSummary, reader),
            SubtaskUrls: p.ReadRelation(ColumnSubtasks, reader),
            ParentUrl: parents.Count > 0 ? parents[0] : null);
    }

    private static TodoStatus MapStatus(string? name) =>
        name is not null && StatusByNotionName.TryGetValue(name, out var s) ? s : TodoStatus.Tache;

    private static TodoPriority? MapPriority(string? name) =>
        name is not null && PriorityByNotionName.TryGetValue(name, out var p) ? p : null;

    private static IReadOnlyList<TodoTag> MapTags(IReadOnlyList<string> names)
    {
        if (names.Count == 0)
        {
            return [];
        }

        var tags = new List<TodoTag>(names.Count);
        foreach (var name in names)
        {
            if (TagByNotionName.TryGetValue(name, out var tag))
            {
                tags.Add(tag);
            }
        }

        return tags;
    }
}
