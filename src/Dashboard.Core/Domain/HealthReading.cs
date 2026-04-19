namespace Dashboard.Core.Domain;

public sealed record HealthReading(
    string Id,
    DateTimeOffset Date,
    double? HeartRateVariability,
    int? RestingHeartRate,
    double? SleepScore);
