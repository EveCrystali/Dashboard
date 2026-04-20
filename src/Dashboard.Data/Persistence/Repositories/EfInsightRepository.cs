using Dashboard.Core.Abstractions.Insights;
using Dashboard.Core.Domain;
using Dashboard.Data.Persistence.Entities;
using Dashboard.Data.Persistence.Mappings;
using Microsoft.EntityFrameworkCore;

namespace Dashboard.Data.Persistence.Repositories;

public sealed class EfInsightRepository : IInsightRepository
{
    private readonly AppDbContext _db;

    public EfInsightRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task StoreSnapshotAsync(IEnumerable<Insight> insights, CancellationToken ct = default)
    {
        var snapshotId = Guid.NewGuid().ToString("N");
        var entities = insights
            .Select(i => InsightEntityMapping.ToEntity(i, snapshotId))
            .ToList();

        if (entities.Count == 0)
        {
            // On persiste un snapshot vide sous forme d'une entrée sentinelle ?
            // Choix : ne rien persister. GetLatestAsync retournera le snapshot
            // précédent. Cela évite d'inonder la table d'entrées vides et
            // reflète l'état "rien à signaler".
            return;
        }

        await _db.Set<InsightEntity>().AddRangeAsync(entities, ct).ConfigureAwait(false);
        await _db.SaveChangesAsync(ct).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<Insight>> GetLatestAsync(CancellationToken ct = default)
    {
        var latestSnapshotId = await _db.Set<InsightEntity>()
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => x.SnapshotId)
            .FirstOrDefaultAsync(ct)
            .ConfigureAwait(false);

        if (latestSnapshotId is null)
        {
            return Array.Empty<Insight>();
        }

        var rows = await _db.Set<InsightEntity>()
            .AsNoTracking()
            .Where(x => x.SnapshotId == latestSnapshotId)
            .OrderByDescending(x => x.Severity)
            .ThenBy(x => x.Title)
            .ToListAsync(ct)
            .ConfigureAwait(false);

        return rows.Select(InsightEntityMapping.ToDomain).ToList();
    }

    public async Task<IReadOnlyList<Insight>> GetHistoryAsync(
        DateTimeOffset from,
        CancellationToken ct = default)
    {
        var rows = await _db.Set<InsightEntity>()
            .AsNoTracking()
            .Where(x => x.CreatedAt >= from)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(ct)
            .ConfigureAwait(false);

        return rows.Select(InsightEntityMapping.ToDomain).ToList();
    }
}
