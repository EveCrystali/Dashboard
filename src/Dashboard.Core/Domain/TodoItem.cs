namespace Dashboard.Core.Domain;

public sealed record TodoItem(
    string Id,
    string Title,
    DateTimeOffset? Due,
    bool IsCompleted);
