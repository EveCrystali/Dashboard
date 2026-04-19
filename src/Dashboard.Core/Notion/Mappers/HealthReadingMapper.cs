using Dashboard.Core.Domain;

namespace Dashboard.Core.Notion.Mappers;

/// <summary>
/// Mapping <see cref="NotionPage"/> → <see cref="HealthReading"/> pour la base
/// « Health Monitor » (data source <c>ff58ea49-daca-41da-9546-adbd93db18d6</c>).
/// </summary>
public static class HealthReadingMapper
{
    public const string ColumnTitle = "Titre";
    public const string ColumnDate = "Date";
    public const string ColumnEntryType = "Type";
    public const string ColumnVerdict = "Verdict";
    public const string ColumnSource = "Source";
    public const string ColumnHrvMs = "HRV ms";
    public const string ColumnRestingHrBpm = "FC Repos bpm";
    public const string ColumnSleepScore = "Score Sommeil";
    public const string ColumnSleepDurationMin = "Dur\u00e9e R\u00e9elle";
    public const string ColumnSleepDeepMin = "Profond min";
    public const string ColumnSleepLightMin = "L\u00e9ger min";
    public const string ColumnSleepRemMin = "REM min";
    public const string ColumnSleepAwakeMin = "\u00c9veil min";
    public const string ColumnWeightKg = "Poids kg";
    public const string ColumnEnergyScore = "\u00c9nergie /100";
    public const string ColumnSpo2Pct = "SpO2 %";
    public const string ColumnStressLowPct = "Stress Bas %";
    public const string ColumnStressHighPct = "Stress Haut %";
    public const string ColumnBloodPressureSys = "TA Sys";
    public const string ColumnBloodPressureDia = "TA Dia";
    public const string ColumnCaffeine = "Caf\u00e9ine";
    public const string ColumnCaloriesDay = "Calories Jour";
    public const string ColumnExerciseTypes = "Exercice Type";
    public const string ColumnExerciseDurationMin = "Exercice Dur\u00e9e min";
    public const string ColumnExerciseCalories = "Exercice Calories";
    public const string ColumnActivityNotes = "Activit\u00e9";
    public const string ColumnWithdrawal = "J+ Sevrage";

    private static readonly IReadOnlyDictionary<string, HealthEntryType> EntryTypeByNotionName = new Dictionary<string, HealthEntryType>(StringComparer.Ordinal)
    {
        ["\ud83d\udcc5 Journ\u00e9e"] = HealthEntryType.Journee,
        ["\ud83c\udf19 Sommeil"] = HealthEntryType.Sommeil,
        ["\ud83d\udc93 Constantes"] = HealthEntryType.Constantes,
        ["\u2696\ufe0f Poids"] = HealthEntryType.Poids,
        ["\ud83e\udde0 Analyse"] = HealthEntryType.Analyse,
        ["\ud83d\udcca Bilan hebdo"] = HealthEntryType.BilanHebdo,
    };

    private static readonly IReadOnlyDictionary<string, HealthVerdict> VerdictByNotionName = new Dictionary<string, HealthVerdict>(StringComparer.Ordinal)
    {
        ["\u2705 Normal"] = HealthVerdict.Normal,
        ["\u26a0\ufe0f Attention"] = HealthVerdict.Attention,
        ["\ud83d\udd34 Alerte"] = HealthVerdict.Alerte,
    };

    private static readonly IReadOnlyDictionary<string, HealthSource> SourceByNotionName = new Dictionary<string, HealthSource>(StringComparer.Ordinal)
    {
        ["Samsung Watch"] = HealthSource.SamsungWatch,
        ["Tensiom\u00e8tre"] = HealthSource.Tensiometre,
        ["Balance"] = HealthSource.Balance,
        ["Manuel"] = HealthSource.Manuel,
        ["Claude Analyse"] = HealthSource.ClaudeAnalyse,
    };

    private static readonly IReadOnlyDictionary<string, ExerciseType> ExerciseByNotionName = new Dictionary<string, ExerciseType>(StringComparer.Ordinal)
    {
        ["Musculation"] = ExerciseType.Musculation,
        ["Running"] = ExerciseType.Running,
        ["Marche"] = ExerciseType.Marche,
        ["Padel"] = ExerciseType.Padel,
        ["VTT"] = ExerciseType.VTT,
        ["Natation"] = ExerciseType.Natation,
        ["Autre"] = ExerciseType.Autre,
    };

    public static HealthReading Map(NotionPage page, INotionPropertyReader reader)
    {
        ArgumentNullException.ThrowIfNull(page);
        ArgumentNullException.ThrowIfNull(reader);

        var p = page.Properties;
        var exerciseNames = p.ReadMultiSelect(ColumnExerciseTypes, reader);

        return new HealthReading(
            Id: page.Id,
            Title: p.ReadTitle(ColumnTitle, reader) ?? string.Empty,
            Date: p.ReadDate(ColumnDate, reader),
            EntryType: LookupNullable(p.ReadSelect(ColumnEntryType, reader), EntryTypeByNotionName),
            Verdict: LookupNullable(p.ReadSelect(ColumnVerdict, reader), VerdictByNotionName),
            Source: LookupNullable(p.ReadSelect(ColumnSource, reader), SourceByNotionName),
            HrvMs: p.ReadNumber(ColumnHrvMs, reader),
            RestingHrBpm: p.ReadNumber(ColumnRestingHrBpm, reader),
            SleepScore: p.ReadNumber(ColumnSleepScore, reader),
            SleepDurationMin: p.ReadNumber(ColumnSleepDurationMin, reader),
            SleepDeepMin: p.ReadNumber(ColumnSleepDeepMin, reader),
            SleepLightMin: p.ReadNumber(ColumnSleepLightMin, reader),
            SleepRemMin: p.ReadNumber(ColumnSleepRemMin, reader),
            SleepAwakeMin: p.ReadNumber(ColumnSleepAwakeMin, reader),
            WeightKg: p.ReadNumber(ColumnWeightKg, reader),
            EnergyScore: p.ReadNumber(ColumnEnergyScore, reader),
            Spo2Pct: p.ReadNumber(ColumnSpo2Pct, reader),
            StressLowPct: p.ReadNumber(ColumnStressLowPct, reader),
            StressHighPct: p.ReadNumber(ColumnStressHighPct, reader),
            BloodPressureSys: p.ReadNumber(ColumnBloodPressureSys, reader),
            BloodPressureDia: p.ReadNumber(ColumnBloodPressureDia, reader),
            CaffeineEspressos: p.ReadNumber(ColumnCaffeine, reader),
            CaloriesDay: p.ReadNumber(ColumnCaloriesDay, reader),
            ExerciseTypes: LookupMulti(exerciseNames, ExerciseByNotionName),
            ExerciseDurationMin: p.ReadNumber(ColumnExerciseDurationMin, reader),
            ExerciseCalories: p.ReadNumber(ColumnExerciseCalories, reader),
            ActivityNotes: p.ReadRichText(ColumnActivityNotes, reader),
            WithdrawalDayPlus: p.ReadNumber(ColumnWithdrawal, reader),
            CreatedTime: page.CreatedTime);
    }

    private static TEnum? LookupNullable<TEnum>(string? name, IReadOnlyDictionary<string, TEnum> map)
        where TEnum : struct, Enum =>
        name is not null && map.TryGetValue(name, out var v) ? v : null;

    private static IReadOnlyList<TEnum> LookupMulti<TEnum>(IReadOnlyList<string> names, IReadOnlyDictionary<string, TEnum> map)
        where TEnum : struct, Enum
    {
        if (names.Count == 0)
        {
            return [];
        }

        var result = new List<TEnum>(names.Count);
        foreach (var n in names)
        {
            if (map.TryGetValue(n, out var v))
            {
                result.Add(v);
            }
        }

        return result;
    }
}
