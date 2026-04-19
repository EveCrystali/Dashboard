using Dashboard.Core.Abstractions;
using Dashboard.Core.Domain;
using Dashboard.Data.Persistence.Entities;
using Dashboard.Data.Persistence.Mappings;
using Microsoft.EntityFrameworkCore;

namespace Dashboard.Data.Persistence.Repositories;

public sealed class JournalEntryRepository : IJournalEntryRepository
{
    private readonly AppDbContext _db;

    public JournalEntryRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<JournalEntry>> GetAllAsync(CancellationToken ct = default)
    {
        var rows = await _db.JournalEntries.AsNoTracking().ToListAsync(ct).ConfigureAwait(false);
        return rows.ConvertAll(JournalEntryEntityMapping.ToDomain);
    }

    public async Task UpsertAsync(JournalEntry item, DateTimeOffset lastEditedTime, CancellationToken ct = default)
    {
        var existing = await _db.JournalEntries.FindAsync([item.Id], ct).ConfigureAwait(false);
        if (existing is null)
        {
            var entity = new JournalEntryEntity();
            JournalEntryEntityMapping.CopyInto(item, lastEditedTime, entity);
            await _db.JournalEntries.AddAsync(entity, ct).ConfigureAwait(false);
        }
        else
        {
            JournalEntryEntityMapping.CopyInto(item, lastEditedTime, existing);
        }
        await _db.SaveChangesAsync(ct).ConfigureAwait(false);
    }

    public async Task<int> DeleteMissingAsync(IReadOnlyCollection<string> presentIds, CancellationToken ct = default)
    {
        var obsolete = await _db.JournalEntries
            .Where(x => !presentIds.Contains(x.Id))
            .ToListAsync(ct)
            .ConfigureAwait(false);
        if (obsolete.Count == 0)
        {
            return 0;
        }
        _db.JournalEntries.RemoveRange(obsolete);
        await _db.SaveChangesAsync(ct).ConfigureAwait(false);
        return obsolete.Count;
    }
}
