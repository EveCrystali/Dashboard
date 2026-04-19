using Dashboard.Core.Domain;

namespace Dashboard.Core.Abstractions;

public interface IHealthReadingRepository
{
    Task<IReadOnlyList<HealthReading>> GetAllAsync(CancellationToken ct = default);

    Task UpsertAsync(HealthReading item, DateTimeOffset lastEditedTime, CancellationToken ct = default);

    Task DeleteMissingAsync(IReadOnlyCollection<string> presentIds, CancellationToken ct = default);
}
