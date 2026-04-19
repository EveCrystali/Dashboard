using Dashboard.Core.Domain;

namespace Dashboard.Data.Persistence.Entities;

public sealed class JournalEntryEntity
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;

    public DateTimeOffset? DateStart { get; set; }
    public DateTimeOffset? DateEnd { get; set; }
    public bool DateIsDateTime { get; set; }

    public JournalType? Type { get; set; }
    public string DomainsJson { get; set; } = "[]";
    public JournalSource? Source { get; set; }
    public DateTimeOffset CreatedTime { get; set; }

    public DateTimeOffset LastEditedTime { get; set; }
}
