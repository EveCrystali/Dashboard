namespace Dashboard.Core.Domain;

public sealed record JournalEntry(
    string Id,
    string Title,
    DateRange? Date,
    JournalType? Type,
    IReadOnlyList<JournalDomain> Domains,
    JournalSource? Source,
    DateTimeOffset CreatedTime);
