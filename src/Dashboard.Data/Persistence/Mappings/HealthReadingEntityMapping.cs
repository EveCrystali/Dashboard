using Dashboard.Core.Domain;
using Dashboard.Data.Persistence.Entities;

namespace Dashboard.Data.Persistence.Mappings;

public static class HealthReadingEntityMapping
{
    public static HealthReading ToDomain(HealthReadingEntity e) =>
        new(
            Id: e.Id,
            Title: e.Title,
            Date: DateRangeMapping.FromColumns(e.DateStart, e.DateEnd, e.DateIsDateTime),
            EntryType: e.EntryType,
            Verdict: e.Verdict,
            Source: e.Source,
            HrvMs: e.HrvMs,
            RestingHrBpm: e.RestingHrBpm,
            SleepScore: e.SleepScore,
            SleepDurationMin: e.SleepDurationMin,
            SleepDeepMin: e.SleepDeepMin,
            SleepLightMin: e.SleepLightMin,
            SleepRemMin: e.SleepRemMin,
            SleepAwakeMin: e.SleepAwakeMin,
            WeightKg: e.WeightKg,
            EnergyScore: e.EnergyScore,
            Spo2Pct: e.Spo2Pct,
            StressLowPct: e.StressLowPct,
            StressHighPct: e.StressHighPct,
            BloodPressureSys: e.BloodPressureSys,
            BloodPressureDia: e.BloodPressureDia,
            CaffeineEspressos: e.CaffeineEspressos,
            CaloriesDay: e.CaloriesDay,
            ExerciseTypes: JsonListSerializer.Deserialize<ExerciseType>(e.ExerciseTypesJson),
            ExerciseDurationMin: e.ExerciseDurationMin,
            ExerciseCalories: e.ExerciseCalories,
            ActivityNotes: e.ActivityNotes,
            WithdrawalDayPlus: e.WithdrawalDayPlus,
            CreatedTime: e.CreatedTime);

    public static void CopyInto(HealthReading item, DateTimeOffset lastEditedTime, HealthReadingEntity target)
    {
        target.Id = item.Id;
        target.Title = item.Title;

        var (dStart, dEnd, dDt) = DateRangeMapping.ToColumns(item.Date);
        target.DateStart = dStart;
        target.DateEnd = dEnd;
        target.DateIsDateTime = dDt;

        target.EntryType = item.EntryType;
        target.Verdict = item.Verdict;
        target.Source = item.Source;
        target.HrvMs = item.HrvMs;
        target.RestingHrBpm = item.RestingHrBpm;
        target.SleepScore = item.SleepScore;
        target.SleepDurationMin = item.SleepDurationMin;
        target.SleepDeepMin = item.SleepDeepMin;
        target.SleepLightMin = item.SleepLightMin;
        target.SleepRemMin = item.SleepRemMin;
        target.SleepAwakeMin = item.SleepAwakeMin;
        target.WeightKg = item.WeightKg;
        target.EnergyScore = item.EnergyScore;
        target.Spo2Pct = item.Spo2Pct;
        target.StressLowPct = item.StressLowPct;
        target.StressHighPct = item.StressHighPct;
        target.BloodPressureSys = item.BloodPressureSys;
        target.BloodPressureDia = item.BloodPressureDia;
        target.CaffeineEspressos = item.CaffeineEspressos;
        target.CaloriesDay = item.CaloriesDay;
        target.ExerciseTypesJson = JsonListSerializer.Serialize(item.ExerciseTypes);
        target.ExerciseDurationMin = item.ExerciseDurationMin;
        target.ExerciseCalories = item.ExerciseCalories;
        target.ActivityNotes = item.ActivityNotes;
        target.WithdrawalDayPlus = item.WithdrawalDayPlus;
        target.CreatedTime = item.CreatedTime;
        target.LastEditedTime = lastEditedTime;
    }
}
