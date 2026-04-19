namespace Dashboard.Core.Abstractions;

public sealed record SyncCursor(
    string DataSourceId,
    DateTimeOffset? LastEditedSeen,
    DateTimeOffset? LastSyncCompleted);

public interface ISyncCursorStore
{
    Task<SyncCursor?> GetAsync(string dataSourceId, CancellationToken ct = default);

    Task UpsertAsync(SyncCursor cursor, CancellationToken ct = default);
}
