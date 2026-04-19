namespace Dashboard.Core.Domain;

public sealed record DateRange(DateTimeOffset? Start, DateTimeOffset? End, bool IsDateTime);
