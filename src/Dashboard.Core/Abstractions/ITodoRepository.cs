using Dashboard.Core.Domain;

namespace Dashboard.Core.Abstractions;

public interface ITodoRepository
{
    Task<IReadOnlyList<TodoItem>> GetAllAsync(CancellationToken ct = default);

    Task UpsertAsync(TodoItem item, DateTimeOffset lastEditedTime, CancellationToken ct = default);

    Task<int> DeleteMissingAsync(IReadOnlyCollection<string> presentIds, CancellationToken ct = default);
}
