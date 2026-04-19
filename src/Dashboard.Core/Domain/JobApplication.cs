namespace Dashboard.Core.Domain;

public sealed record JobApplication(
    string Id,
    string Company,
    string Role,
    string Status,
    DateTimeOffset AppliedAt);
