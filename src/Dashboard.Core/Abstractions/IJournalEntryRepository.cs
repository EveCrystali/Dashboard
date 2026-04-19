using Dashboard.Core.Domain;

namespace Dashboard.Core.Abstractions;

public interface IJournalEntryRepository
{
    Task<IReadOnlyList<JournalEntry>> GetAllAsync(CancellationToken ct = default);

    Task UpsertAsync(JournalEntry item, DateTimeOffset lastEditedTime, CancellationToken ct = default);

    Task<int> DeleteMissingAsync(IReadOnlyCollection<string> presentIds, CancellationToken ct = default);
}
