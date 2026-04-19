using Dashboard.Core.Domain;

namespace Dashboard.Core.Abstractions;

public interface IJobApplicationRepository
{
    Task<IReadOnlyList<JobApplication>> GetAllAsync(CancellationToken ct = default);

    Task UpsertAsync(JobApplication item, DateTimeOffset lastEditedTime, CancellationToken ct = default);

    Task<int> DeleteMissingAsync(IReadOnlyCollection<string> presentIds, CancellationToken ct = default);
}
