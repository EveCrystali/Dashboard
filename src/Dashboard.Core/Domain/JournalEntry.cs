namespace Dashboard.Core.Domain;

public sealed record JournalEntry(
    string Id,
    DateTimeOffset Date,
    string Title,
    string Content);
