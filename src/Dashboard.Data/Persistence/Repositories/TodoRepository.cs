using Dashboard.Core.Abstractions;
using Dashboard.Core.Domain;
using Dashboard.Data.Persistence.Entities;
using Dashboard.Data.Persistence.Mappings;
using Microsoft.EntityFrameworkCore;

namespace Dashboard.Data.Persistence.Repositories;

public sealed class TodoRepository : ITodoRepository
{
    private readonly AppDbContext _db;

    public TodoRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<TodoItem>> GetAllAsync(CancellationToken ct = default)
    {
        var rows = await _db.Todos.AsNoTracking().ToListAsync(ct).ConfigureAwait(false);
        return rows.ConvertAll(TodoEntityMapping.ToDomain);
    }

    public async Task UpsertAsync(TodoItem item, DateTimeOffset lastEditedTime, CancellationToken ct = default)
    {
        var existing = await _db.Todos.FindAsync([item.Id], ct).ConfigureAwait(false);
        if (existing is null)
        {
            var entity = new TodoEntity();
            TodoEntityMapping.CopyInto(item, lastEditedTime, entity);
            await _db.Todos.AddAsync(entity, ct).ConfigureAwait(false);
        }
        else
        {
            TodoEntityMapping.CopyInto(item, lastEditedTime, existing);
        }
        await _db.SaveChangesAsync(ct).ConfigureAwait(false);
    }

    public async Task DeleteMissingAsync(IReadOnlyCollection<string> presentIds, CancellationToken ct = default)
    {
        var obsolete = await _db.Todos
            .Where(x => !presentIds.Contains(x.Id))
            .ToListAsync(ct)
            .ConfigureAwait(false);
        if (obsolete.Count == 0)
        {
            return;
        }
        _db.Todos.RemoveRange(obsolete);
        await _db.SaveChangesAsync(ct).ConfigureAwait(false);
    }
}
