namespace Dashboard.Core.Notion;

/// <summary>
/// Item de domaine accompagné de la métadonnée <c>last_edited_time</c> issue
/// de Notion. Permet à <c>SyncOrchestrator</c> d'alimenter
/// <c>UpsertAsync(item, lastEditedTime)</c> sans polluer les records de domaine.
/// </summary>
public sealed record NotionSnapshot<T>(T Item, DateTimeOffset LastEditedTime);
