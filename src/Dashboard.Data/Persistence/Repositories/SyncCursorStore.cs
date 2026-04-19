using Dashboard.Core.Abstractions;
using Dashboard.Data.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Dashboard.Data.Persistence.Repositories;

public sealed class SyncCursorStore : ISyncCursorStore
{
    private readonly AppDbContext _db;

    public SyncCursorStore(AppDbContext db)
    {
        _db = db;
    }

    public async Task<SyncCursor?> GetAsync(string dataSourceId, CancellationToken ct = default)
    {
        var row = await _db.SyncCursors
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.DataSourceId == dataSourceId, ct)
            .ConfigureAwait(false);
        return row is null
            ? null
            : new SyncCursor(row.DataSourceId, row.LastEditedSeen, row.LastSyncCompleted);
    }

    public async Task UpsertAsync(SyncCursor cursor, CancellationToken ct = default)
    {
        var existing = await _db.SyncCursors
            .FirstOrDefaultAsync(x => x.DataSourceId == cursor.DataSourceId, ct)
            .ConfigureAwait(false);
        if (existing is null)
        {
            await _db.SyncCursors.AddAsync(new SyncCursorEntity
            {
                DataSourceId = cursor.DataSourceId,
                LastEditedSeen = cursor.LastEditedSeen,
                LastSyncCompleted = cursor.LastSyncCompleted,
            }, ct).ConfigureAwait(false);
        }
        else
        {
            existing.LastEditedSeen = cursor.LastEditedSeen;
            existing.LastSyncCompleted = cursor.LastSyncCompleted;
        }
        await _db.SaveChangesAsync(ct).ConfigureAwait(false);
    }
}
