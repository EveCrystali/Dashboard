using Dashboard.Core.Domain;
using Dashboard.Core.Notion;
using Dashboard.Core.Notion.Mappers;

namespace Dashboard.Core.Tests.Notion.Mappers;

public sealed class HealthReadingMapperTests
{
    private static readonly NotionPropertyReader Reader = new();

    [Fact]
    public void Map_remplit_les_metriques_d_une_lecture_sommeil()
    {
        var page = FixtureLoader.LoadPage("health-reading-page.json");

        var reading = HealthReadingMapper.Map(page, Reader);

        reading.Id.Should().Be("dddddddd-dddd-dddd-dddd-dddddddddddd");
        reading.Title.Should().Be("Sommeil 18/04");
        reading.EntryType.Should().Be(HealthEntryType.Sommeil);
        reading.Verdict.Should().Be(HealthVerdict.Normal);
        reading.Source.Should().Be(HealthSource.SamsungWatch);
        reading.HrvMs.Should().Be(58.2);
        reading.RestingHrBpm.Should().Be(55);
        reading.SleepScore.Should().Be(82);
        reading.SleepDurationMin.Should().Be(425);
        reading.SleepDeepMin.Should().Be(90);
        reading.SleepRemMin.Should().Be(70);
        reading.SleepAwakeMin.Should().Be(20);
        reading.WeightKg.Should().BeNull();
        reading.EnergyScore.Should().Be(78);
        reading.Spo2Pct.Should().Be(96);
        reading.BloodPressureSys.Should().BeNull();
        reading.CaffeineEspressos.Should().Be(2);
        reading.ExerciseTypes.Should().BeEmpty();
        reading.ActivityNotes.Should().BeNull();
        reading.WithdrawalDayPlus.Should().Be(29);
        reading.Date.Should().NotBeNull();
        reading.Date!.Start.Should().Be(new DateTimeOffset(2026, 4, 18, 0, 0, 0, TimeSpan.Zero));
    }
}
