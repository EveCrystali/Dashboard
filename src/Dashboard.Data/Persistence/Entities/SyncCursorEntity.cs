namespace Dashboard.Data.Persistence.Entities;

public sealed class SyncCursorEntity
{
    public string DataSourceId { get; set; } = string.Empty;
    public DateTimeOffset? LastEditedSeen { get; set; }
    public DateTimeOffset? LastSyncCompleted { get; set; }
}
