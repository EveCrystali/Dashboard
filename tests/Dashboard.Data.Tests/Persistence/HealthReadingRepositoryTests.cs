using Dashboard.Core.Domain;
using Dashboard.Data.Persistence.Repositories;

namespace Dashboard.Data.Tests.Persistence;

public sealed class HealthReadingRepositoryTests
{
    [Fact]
    public async Task RoundTrip_conserve_metriques_et_exercices()
    {
        using var fixture = new SqliteInMemoryFixture();

        var original = new HealthReading(
            Id: "h-1",
            Title: "Journée 19 avril",
            Date: new DateRange(new DateTimeOffset(2026, 4, 19, 0, 0, 0, TimeSpan.Zero), null, false),
            EntryType: HealthEntryType.Journee,
            Verdict: HealthVerdict.Normal,
            Source: HealthSource.SamsungWatch,
            HrvMs: 48,
            RestingHrBpm: 54,
            SleepScore: 82,
            SleepDurationMin: 440,
            SleepDeepMin: 90,
            SleepLightMin: 220,
            SleepRemMin: 110,
            SleepAwakeMin: 20,
            WeightKg: 72.5,
            EnergyScore: 78,
            Spo2Pct: 97,
            StressLowPct: 65,
            StressHighPct: 10,
            BloodPressureSys: 120,
            BloodPressureDia: 78,
            CaffeineEspressos: 2,
            CaloriesDay: 2200,
            ExerciseTypes: [ExerciseType.Musculation, ExerciseType.Marche],
            ExerciseDurationMin: 45,
            ExerciseCalories: 320,
            ActivityNotes: "Bonne forme",
            WithdrawalDayPlus: null,
            CreatedTime: new DateTimeOffset(2026, 4, 19, 7, 0, 0, TimeSpan.Zero));

        using (var ctx = fixture.CreateContext())
        {
            var repo = new HealthReadingRepository(ctx);
            await repo.UpsertAsync(original, DateTimeOffset.UtcNow);
        }

        using (var ctx = fixture.CreateContext())
        {
            var repo = new HealthReadingRepository(ctx);
            var roundTrip = (await repo.GetAllAsync()).Single();
            roundTrip.Should().BeEquivalentTo(original);
        }
    }
}
