using Dashboard.Core.Abstractions.Calendar;
using Dashboard.Core.Services;
using Moq;

namespace Dashboard.Core.Tests.Services;

public sealed class AndroidCalendarServiceTests
{
    private static readonly DateTimeOffset From = new(2026, 4, 19, 0, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset To = new(2026, 4, 26, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task GetEventsAsync_retourne_vide_si_permission_refusee()
    {
        var reader = new FakeReader();
        var perms = new Mock<ICalendarPermissionRequester>();
        perms.Setup(p => p.IsGrantedAsync(It.IsAny<CancellationToken>())).ReturnsAsync(false);
        var sut = new AndroidCalendarService(reader, perms.Object);

        var events = await sut.GetEventsAsync(From, To);

        events.Should().BeEmpty();
        reader.CalendarsReadCount.Should().Be(0);
        reader.InstancesReadCount.Should().Be(0);
    }

    [Fact]
    public async Task GetEventsAsync_mappe_les_lignes_brutes_vers_le_domaine()
    {
        var begin = new DateTimeOffset(2026, 4, 20, 9, 0, 0, TimeSpan.Zero).ToUnixTimeMilliseconds();
        var end = new DateTimeOffset(2026, 4, 20, 10, 30, 0, TimeSpan.Zero).ToUnixTimeMilliseconds();
        var reader = new FakeReader
        {
            Calendars = { new RawCalendarRow(42, "Perso", "antoine@example.com", 0xFF1F4E9D, true) },
            Instances = { new RawEventRow(7, 42, "Standup", begin, end, false, "Europe/Paris") },
        };
        var sut = BuildGrantedSut(reader);

        var events = await sut.GetEventsAsync(From, To);

        var single = events.Should().ContainSingle().Subject;
        single.Id.Should().Be("7");
        single.CalendarId.Should().Be("42");
        single.CalendarDisplayName.Should().Be("Perso");
        single.Title.Should().Be("Standup");
        single.Start.Should().Be(DateTimeOffset.FromUnixTimeMilliseconds(begin));
        single.End.Should().Be(DateTimeOffset.FromUnixTimeMilliseconds(end));
        single.IsAllDay.Should().BeFalse();
    }

    [Fact]
    public async Task GetEventsAsync_trie_par_Start_ascendant()
    {
        var t1 = new DateTimeOffset(2026, 4, 20, 9, 0, 0, TimeSpan.Zero).ToUnixTimeMilliseconds();
        var t2 = new DateTimeOffset(2026, 4, 21, 9, 0, 0, TimeSpan.Zero).ToUnixTimeMilliseconds();
        var t3 = new DateTimeOffset(2026, 4, 22, 9, 0, 0, TimeSpan.Zero).ToUnixTimeMilliseconds();
        var reader = new FakeReader
        {
            Calendars = { new RawCalendarRow(1, "Perso", null, 0, true) },
            Instances =
            {
                new RawEventRow(20, 1, "Mardi", t2, t2 + 3_600_000, false, null),
                new RawEventRow(10, 1, "Lundi", t1, t1 + 3_600_000, false, null),
                new RawEventRow(30, 1, "Mercredi", t3, t3 + 3_600_000, false, null),
            },
        };
        var sut = BuildGrantedSut(reader);

        var events = await sut.GetEventsAsync(From, To);

        events.Select(e => e.Title).Should().Equal("Lundi", "Mardi", "Mercredi");
    }

    [Fact]
    public async Task GetEventsAsync_propage_le_flag_all_day_et_le_titre_null_devient_chaine_vide()
    {
        var begin = new DateTimeOffset(2026, 4, 20, 0, 0, 0, TimeSpan.Zero).ToUnixTimeMilliseconds();
        var end = new DateTimeOffset(2026, 4, 21, 0, 0, 0, TimeSpan.Zero).ToUnixTimeMilliseconds();
        var reader = new FakeReader
        {
            Calendars = { new RawCalendarRow(1, "Perso", null, 0, true) },
            Instances = { new RawEventRow(99, 1, null, begin, end, true, "UTC") },
        };
        var sut = BuildGrantedSut(reader);

        var events = await sut.GetEventsAsync(From, To);

        var single = events.Should().ContainSingle().Subject;
        single.IsAllDay.Should().BeTrue();
        single.Title.Should().BeEmpty();
    }

    [Fact]
    public async Task GetEventsAsync_evenement_orphelin_calendrier_inconnu_recoit_displayName_vide()
    {
        var begin = new DateTimeOffset(2026, 4, 20, 9, 0, 0, TimeSpan.Zero).ToUnixTimeMilliseconds();
        var reader = new FakeReader
        {
            Calendars = { new RawCalendarRow(1, "Perso", null, 0, true) },
            Instances = { new RawEventRow(50, 999, "Orphelin", begin, begin + 3_600_000, false, null) },
        };
        var sut = BuildGrantedSut(reader);

        var events = await sut.GetEventsAsync(From, To);

        events.Should().ContainSingle().Which.CalendarDisplayName.Should().BeEmpty();
    }

    [Fact]
    public async Task GetEventsAsync_propage_la_fenetre_temporelle_au_reader()
    {
        var reader = new FakeReader();
        var sut = BuildGrantedSut(reader);

        await sut.GetEventsAsync(From, To);

        reader.LastReadFrom.Should().Be(From);
        reader.LastReadTo.Should().Be(To);
    }

    private static AndroidCalendarService BuildGrantedSut(ICalendarContentReader reader)
    {
        var perms = new Mock<ICalendarPermissionRequester>();
        perms.Setup(p => p.IsGrantedAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);
        return new AndroidCalendarService(reader, perms.Object);
    }

    private sealed class FakeReader : ICalendarContentReader
    {
        public List<RawCalendarRow> Calendars { get; } = new();
        public List<RawEventRow> Instances { get; } = new();
        public int CalendarsReadCount { get; private set; }
        public int InstancesReadCount { get; private set; }
        public DateTimeOffset? LastReadFrom { get; private set; }
        public DateTimeOffset? LastReadTo { get; private set; }

        public IEnumerable<RawCalendarRow> ReadCalendars()
        {
            CalendarsReadCount++;
            return Calendars;
        }

        public IEnumerable<RawEventRow> ReadInstances(DateTimeOffset from, DateTimeOffset to)
        {
            InstancesReadCount++;
            LastReadFrom = from;
            LastReadTo = to;
            return Instances;
        }
    }
}
