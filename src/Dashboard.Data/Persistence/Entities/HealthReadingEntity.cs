using Dashboard.Core.Domain;

namespace Dashboard.Data.Persistence.Entities;

public sealed class HealthReadingEntity
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;

    public DateTimeOffset? DateStart { get; set; }
    public DateTimeOffset? DateEnd { get; set; }
    public bool DateIsDateTime { get; set; }

    public HealthEntryType? EntryType { get; set; }
    public HealthVerdict? Verdict { get; set; }
    public HealthSource? Source { get; set; }

    public double? HrvMs { get; set; }
    public double? RestingHrBpm { get; set; }
    public double? SleepScore { get; set; }
    public double? SleepDurationMin { get; set; }
    public double? SleepDeepMin { get; set; }
    public double? SleepLightMin { get; set; }
    public double? SleepRemMin { get; set; }
    public double? SleepAwakeMin { get; set; }
    public double? WeightKg { get; set; }
    public double? EnergyScore { get; set; }
    public double? Spo2Pct { get; set; }
    public double? StressLowPct { get; set; }
    public double? StressHighPct { get; set; }
    public double? BloodPressureSys { get; set; }
    public double? BloodPressureDia { get; set; }
    public double? CaffeineEspressos { get; set; }
    public double? CaloriesDay { get; set; }
    public string ExerciseTypesJson { get; set; } = "[]";
    public double? ExerciseDurationMin { get; set; }
    public double? ExerciseCalories { get; set; }
    public string? ActivityNotes { get; set; }
    public double? WithdrawalDayPlus { get; set; }

    public DateTimeOffset CreatedTime { get; set; }
    public DateTimeOffset LastEditedTime { get; set; }
}
