namespace Dashboard.Core.Abstractions;

public sealed record SyncSourceResult(
    string DataSourceId,
    bool Success,
    bool WasFullSync,
    int Upserts,
    int Deletes,
    string? ErrorMessage)
{
    public static SyncSourceResult Ok(string dataSourceId, bool wasFullSync, int upserts, int deletes) =>
        new(dataSourceId, true, wasFullSync, upserts, deletes, null);

    public static SyncSourceResult Failure(string dataSourceId, bool wasFullSync, int upserts, string error) =>
        new(dataSourceId, false, wasFullSync, upserts, 0, error);
}

public sealed record SyncReport(IReadOnlyList<SyncSourceResult> Results)
{
    public bool AllSucceeded => Results.All(r => r.Success);
}
