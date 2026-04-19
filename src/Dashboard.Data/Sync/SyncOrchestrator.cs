using Dashboard.Core.Abstractions;
using Dashboard.Core.Domain;
using Dashboard.Core.Notion;
using Dashboard.Data.Notion;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Dashboard.Data.Sync;

/// <summary>
/// Orchestre la synchronisation différentielle des 4 data sources Notion vers
/// le cache local. Stratégie par source :
/// <list type="bullet">
/// <item>Pas de curseur ou dernière sync complète &gt; 6 h : full sync (sans
/// filtre Notion) puis <c>DeleteMissingAsync</c> pour aligner les suppressions.</item>
/// <item>Sinon : delta sync via <c>filter.last_edited_time.on_or_after</c>,
/// sans détection de suppression.</item>
/// </list>
/// Les erreurs sont isolées par source : une panne sur Todos n'empêche pas
/// les Candidatures, le Journal et la Santé d'aboutir. Le rapport agrégé
/// indique les succès et échecs partiels.
/// </summary>
public sealed class SyncOrchestrator : ISyncOrchestrator
{
    internal static readonly TimeSpan FullSyncWindow = TimeSpan.FromHours(6);

    private readonly INotionService _notion;
    private readonly ITodoRepository _todos;
    private readonly IJobApplicationRepository _jobs;
    private readonly IJournalEntryRepository _journal;
    private readonly IHealthReadingRepository _health;
    private readonly ISyncCursorStore _cursors;
    private readonly IClock _clock;
    private readonly IOptions<NotionOptions> _options;
    private readonly ILogger<SyncOrchestrator> _logger;

    public SyncOrchestrator(
        INotionService notion,
        ITodoRepository todos,
        IJobApplicationRepository jobs,
        IJournalEntryRepository journal,
        IHealthReadingRepository health,
        ISyncCursorStore cursors,
        IClock clock,
        IOptions<NotionOptions> options,
        ILogger<SyncOrchestrator> logger)
    {
        _notion = notion;
        _todos = todos;
        _jobs = jobs;
        _journal = journal;
        _health = health;
        _cursors = cursors;
        _clock = clock;
        _options = options;
        _logger = logger;
    }

    public async Task<SyncReport> SyncAllAsync(CancellationToken ct = default)
    {
        var ds = _options.Value.DataSources;
        var results = new List<SyncSourceResult>(4)
        {
            await SyncOneAsync(
                ds.Todos,
                _notion.GetTodosAsync,
                _todos.UpsertAsync,
                _todos.DeleteMissingAsync,
                static t => t.Id,
                ct).ConfigureAwait(false),
            await SyncOneAsync(
                ds.JobApplications,
                _notion.GetJobApplicationsAsync,
                _jobs.UpsertAsync,
                _jobs.DeleteMissingAsync,
                static j => j.Id,
                ct).ConfigureAwait(false),
            await SyncOneAsync(
                ds.Journal,
                _notion.GetJournalEntriesAsync,
                _journal.UpsertAsync,
                _journal.DeleteMissingAsync,
                static j => j.Id,
                ct).ConfigureAwait(false),
            await SyncOneAsync(
                ds.Health,
                _notion.GetHealthReadingsAsync,
                _health.UpsertAsync,
                _health.DeleteMissingAsync,
                static h => h.Id,
                ct).ConfigureAwait(false),
        };
        return new SyncReport(results);
    }

    private async Task<SyncSourceResult> SyncOneAsync<T>(
        string dataSourceId,
        Func<DateTimeOffset?, CancellationToken, IAsyncEnumerable<NotionSnapshot<T>>> fetch,
        Func<T, DateTimeOffset, CancellationToken, Task> upsert,
        Func<IReadOnlyCollection<string>, CancellationToken, Task<int>> deleteMissing,
        Func<T, string> getId,
        CancellationToken ct)
    {
        SyncCursor? cursor;
        try
        {
            cursor = await _cursors.GetAsync(dataSourceId, ct).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogWarning(ex, "Lecture du curseur pour {DataSourceId} a échoué.", dataSourceId);
            return SyncSourceResult.Failure(dataSourceId, false, 0, ex.Message);
        }

        var now = _clock.Now;
        bool fullSync = cursor is null
            || cursor.LastSyncCompleted is null
            || now - cursor.LastSyncCompleted.Value >= FullSyncWindow;

        var filter = fullSync ? null : cursor!.LastEditedSeen;
        HashSet<string>? seenIds = fullSync ? new HashSet<string>(StringComparer.Ordinal) : null;
        DateTimeOffset? maxEdited = cursor?.LastEditedSeen;
        int upserts = 0;

        try
        {
            await foreach (var snapshot in fetch(filter, ct).ConfigureAwait(false))
            {
                await upsert(snapshot.Item, snapshot.LastEditedTime, ct).ConfigureAwait(false);
                upserts++;
                seenIds?.Add(getId(snapshot.Item));
                if (maxEdited is null || snapshot.LastEditedTime > maxEdited.Value)
                {
                    maxEdited = snapshot.LastEditedTime;
                }
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogWarning(ex, "Sync {DataSourceId} a échoué (fullSync={FullSync}, upserts effectués={Upserts}).", dataSourceId, fullSync, upserts);
            return SyncSourceResult.Failure(dataSourceId, fullSync, upserts, ex.Message);
        }

        int deletes = 0;
        if (fullSync && seenIds is not null)
        {
            try
            {
                deletes = await deleteMissing(seenIds, ct).ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogWarning(ex, "DeleteMissing {DataSourceId} a échoué.", dataSourceId);
                return SyncSourceResult.Failure(dataSourceId, fullSync, upserts, ex.Message);
            }
        }

        try
        {
            await _cursors.UpsertAsync(new SyncCursor(
                dataSourceId,
                LastEditedSeen: maxEdited,
                LastSyncCompleted: fullSync ? now : cursor?.LastSyncCompleted),
                ct).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogWarning(ex, "Mise à jour du curseur {DataSourceId} a échoué.", dataSourceId);
            return SyncSourceResult.Failure(dataSourceId, fullSync, upserts, ex.Message);
        }

        return SyncSourceResult.Ok(dataSourceId, fullSync, upserts, deletes);
    }
}
