using Dashboard.Core.Abstractions;
using Dashboard.Core.Domain;
using Dashboard.Core.Services.Insights.Rules;
using Moq;

namespace Dashboard.Core.Tests.Services.Insights.Rules;

public sealed class HealthMonitorStaleOver48hRuleTests
{
    private static readonly DateTimeOffset Now = new(2026, 4, 20, 10, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Aucune_donnee_emet_Critical()
    {
        var sut = BuildSut([]);

        var insights = await sut.EvaluateAsync();

        var single = insights.Should().ContainSingle().Subject;
        single.Severity.Should().Be(InsightSeverity.Critical);
        single.Title.Should().Contain("jamais renseigné");
    }

    [Fact]
    public async Task Donnee_recente_aucun_insight()
    {
        var reading = MakeReading("r1", date: Now.AddHours(-10));
        var sut = BuildSut([reading]);

        var insights = await sut.EvaluateAsync();

        insights.Should().BeEmpty();
    }

    [Fact]
    public async Task Donnee_ancienne_au_dela_de_48h_emet_Critical()
    {
        var reading = MakeReading("r1", date: Now.AddHours(-60));
        var sut = BuildSut([reading]);

        var insights = await sut.EvaluateAsync();

        var single = insights.Should().ContainSingle().Subject;
        single.Severity.Should().Be(InsightSeverity.Critical);
        single.Detail.Should().Contain("60").And.Contain("48");
    }

    [Fact]
    public async Task Utilise_CreatedTime_si_Date_absent()
    {
        var reading = MakeReading("r1", date: null, createdTime: Now.AddHours(-5));
        var sut = BuildSut([reading]);

        var insights = await sut.EvaluateAsync();

        insights.Should().BeEmpty();
    }

    [Fact]
    public async Task Utilise_le_max_entre_plusieurs_lectures()
    {
        var readings = new List<HealthReading>
        {
            MakeReading("r1", date: Now.AddHours(-100)),
            MakeReading("r2", date: Now.AddHours(-5)),
            MakeReading("r3", date: Now.AddHours(-200)),
        };
        var sut = BuildSut(readings);

        var insights = await sut.EvaluateAsync();

        insights.Should().BeEmpty();
    }

    private static HealthMonitorStaleOver48hRule BuildSut(IReadOnlyList<HealthReading> readings)
    {
        var repo = new Mock<IHealthReadingRepository>();
        repo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(readings);
        var clock = new Mock<IClock>();
        clock.SetupGet(c => c.Now).Returns(Now);
        return new HealthMonitorStaleOver48hRule(repo.Object, clock.Object);
    }

    private static HealthReading MakeReading(
        string id,
        DateTimeOffset? date,
        DateTimeOffset? createdTime = null) =>
        new(
            Id: id,
            Title: "Lecture",
            Date: date.HasValue ? new DateRange(date, null, true) : null,
            EntryType: null,
            Verdict: null,
            Source: null,
            HrvMs: null,
            RestingHrBpm: null,
            SleepScore: null,
            SleepDurationMin: null,
            SleepDeepMin: null,
            SleepLightMin: null,
            SleepRemMin: null,
            SleepAwakeMin: null,
            WeightKg: null,
            EnergyScore: null,
            Spo2Pct: null,
            StressLowPct: null,
            StressHighPct: null,
            BloodPressureSys: null,
            BloodPressureDia: null,
            CaffeineEspressos: null,
            CaloriesDay: null,
            ExerciseTypes: Array.Empty<ExerciseType>(),
            ExerciseDurationMin: null,
            ExerciseCalories: null,
            ActivityNotes: null,
            WithdrawalDayPlus: null,
            CreatedTime: createdTime ?? DateTimeOffset.MinValue);
}
