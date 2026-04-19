using Dashboard.Core.Abstractions;
using Dashboard.Core.Domain;
using Dashboard.Data.Persistence.Entities;
using Dashboard.Data.Persistence.Mappings;
using Microsoft.EntityFrameworkCore;

namespace Dashboard.Data.Persistence.Repositories;

public sealed class JobApplicationRepository : IJobApplicationRepository
{
    private readonly AppDbContext _db;

    public JobApplicationRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<JobApplication>> GetAllAsync(CancellationToken ct = default)
    {
        var rows = await _db.JobApplications.AsNoTracking().ToListAsync(ct).ConfigureAwait(false);
        return rows.ConvertAll(JobApplicationEntityMapping.ToDomain);
    }

    public async Task UpsertAsync(JobApplication item, DateTimeOffset lastEditedTime, CancellationToken ct = default)
    {
        var existing = await _db.JobApplications.FindAsync([item.Id], ct).ConfigureAwait(false);
        if (existing is null)
        {
            var entity = new JobApplicationEntity();
            JobApplicationEntityMapping.CopyInto(item, lastEditedTime, entity);
            await _db.JobApplications.AddAsync(entity, ct).ConfigureAwait(false);
        }
        else
        {
            JobApplicationEntityMapping.CopyInto(item, lastEditedTime, existing);
        }
        await _db.SaveChangesAsync(ct).ConfigureAwait(false);
    }

    public async Task DeleteMissingAsync(IReadOnlyCollection<string> presentIds, CancellationToken ct = default)
    {
        var obsolete = await _db.JobApplications
            .Where(x => !presentIds.Contains(x.Id))
            .ToListAsync(ct)
            .ConfigureAwait(false);
        if (obsolete.Count == 0)
        {
            return;
        }
        _db.JobApplications.RemoveRange(obsolete);
        await _db.SaveChangesAsync(ct).ConfigureAwait(false);
    }
}
