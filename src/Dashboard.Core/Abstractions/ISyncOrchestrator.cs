namespace Dashboard.Core.Abstractions;

/// <summary>
/// Orchestre la synchronisation différentielle des 4 data sources Notion vers
/// le cache local. Met à jour <see cref="ISyncCursorStore"/> à chaque source
/// traitée. Une erreur sur une source n'interrompt pas les autres : le rapport
/// agrégé indique les succès et échecs partiels.
/// </summary>
public interface ISyncOrchestrator
{
    Task<SyncReport> SyncAllAsync(CancellationToken ct = default);
}
