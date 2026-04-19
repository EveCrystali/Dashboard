using Dashboard.Core.Abstractions;
using Dashboard.Core.Domain;
using Dashboard.Data.Persistence.Entities;
using Dashboard.Data.Persistence.Mappings;
using Microsoft.EntityFrameworkCore;

namespace Dashboard.Data.Persistence.Repositories;

public sealed class HealthReadingRepository : IHealthReadingRepository
{
    private readonly AppDbContext _db;

    public HealthReadingRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<HealthReading>> GetAllAsync(CancellationToken ct = default)
    {
        var rows = await _db.HealthReadings.AsNoTracking().ToListAsync(ct).ConfigureAwait(false);
        return rows.ConvertAll(HealthReadingEntityMapping.ToDomain);
    }

    public async Task UpsertAsync(HealthReading item, DateTimeOffset lastEditedTime, CancellationToken ct = default)
    {
        var existing = await _db.HealthReadings.FindAsync([item.Id], ct).ConfigureAwait(false);
        if (existing is null)
        {
            var entity = new HealthReadingEntity();
            HealthReadingEntityMapping.CopyInto(item, lastEditedTime, entity);
            await _db.HealthReadings.AddAsync(entity, ct).ConfigureAwait(false);
        }
        else
        {
            HealthReadingEntityMapping.CopyInto(item, lastEditedTime, existing);
        }
        await _db.SaveChangesAsync(ct).ConfigureAwait(false);
    }

    public async Task DeleteMissingAsync(IReadOnlyCollection<string> presentIds, CancellationToken ct = default)
    {
        var obsolete = await _db.HealthReadings
            .Where(x => !presentIds.Contains(x.Id))
            .ToListAsync(ct)
            .ConfigureAwait(false);
        if (obsolete.Count == 0)
        {
            return;
        }
        _db.HealthReadings.RemoveRange(obsolete);
        await _db.SaveChangesAsync(ct).ConfigureAwait(false);
    }
}
