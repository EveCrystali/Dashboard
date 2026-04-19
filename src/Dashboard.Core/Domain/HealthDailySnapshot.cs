namespace Dashboard.Core.Domain;

public sealed record HealthDailySnapshot(
    DateTimeOffset Date,
    double? HrvMs,
    double? RestingHrBpm,
    double? SleepScore,
    double? WeightKg,
    HealthVerdict? Verdict,
    IReadOnlyList<HealthAlert> Alerts);

public sealed record HealthAlert(string Metric, double Value, double Threshold, string Message);
