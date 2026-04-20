using Dashboard.Core.Domain;
using Dashboard.Data.Persistence.Repositories;

namespace Dashboard.Data.Tests.Persistence;

public sealed class EfInsightRepositoryTests
{
    private static readonly DateTimeOffset T1 = new(2026, 4, 19, 10, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset T2 = new(2026, 4, 20, 10, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task GetLatestAsync_retourne_vide_si_aucun_snapshot()
    {
        using var fixture = new SqliteInMemoryFixture();
        using var ctx = fixture.CreateContext();
        var sut = new EfInsightRepository(ctx);

        var latest = await sut.GetLatestAsync();

        latest.Should().BeEmpty();
    }

    [Fact]
    public async Task StoreSnapshotAsync_vide_ne_persiste_rien()
    {
        using var fixture = new SqliteInMemoryFixture();
        using (var ctx = fixture.CreateContext())
        {
            var sut = new EfInsightRepository(ctx);
            await sut.StoreSnapshotAsync([]);
        }

        using (var ctx = fixture.CreateContext())
        {
            var sut = new EfInsightRepository(ctx);
            var latest = await sut.GetLatestAsync();
            latest.Should().BeEmpty();
        }
    }

    [Fact]
    public async Task StoreSnapshotAsync_puis_GetLatest_rend_le_dernier_snapshot_seulement()
    {
        using var fixture = new SqliteInMemoryFixture();

        using (var ctx = fixture.CreateContext())
        {
            var sut = new EfInsightRepository(ctx);
            await sut.StoreSnapshotAsync([MakeInsight("a1", T1), MakeInsight("a2", T1)]);
            await sut.StoreSnapshotAsync([MakeInsight("b1", T2)]);
        }

        using (var ctx = fixture.CreateContext())
        {
            var sut = new EfInsightRepository(ctx);
            var latest = await sut.GetLatestAsync();

            latest.Should().ContainSingle().Which.Id.Should().Be("b1");
        }
    }

    [Fact]
    public async Task GetLatest_ordonne_par_severity_descendant_puis_titre()
    {
        using var fixture = new SqliteInMemoryFixture();

        using (var ctx = fixture.CreateContext())
        {
            var sut = new EfInsightRepository(ctx);
            await sut.StoreSnapshotAsync([
                MakeInsight("i1", T1, severity: InsightSeverity.Info, title: "Info A"),
                MakeInsight("i2", T1, severity: InsightSeverity.Critical, title: "Critical B"),
                MakeInsight("i3", T1, severity: InsightSeverity.Warning, title: "Warning C"),
            ]);
        }

        using (var ctx = fixture.CreateContext())
        {
            var sut = new EfInsightRepository(ctx);
            var latest = await sut.GetLatestAsync();

            latest.Select(i => i.Id).Should().Equal("i2", "i3", "i1");
        }
    }

    [Fact]
    public async Task GetHistoryAsync_filtre_par_date_inclusif()
    {
        using var fixture = new SqliteInMemoryFixture();

        using (var ctx = fixture.CreateContext())
        {
            var sut = new EfInsightRepository(ctx);
            await sut.StoreSnapshotAsync([MakeInsight("old", T1)]);
            await sut.StoreSnapshotAsync([MakeInsight("new", T2)]);
        }

        using (var ctx = fixture.CreateContext())
        {
            var sut = new EfInsightRepository(ctx);
            var history = await sut.GetHistoryAsync(T2);

            history.Select(i => i.Id).Should().Equal("new");
        }
    }

    [Fact]
    public async Task StoreSnapshotAsync_round_trip_preserve_tous_les_champs()
    {
        using var fixture = new SqliteInMemoryFixture();

        var source = new Insight(
            Id: "round",
            RuleId: "rule-x",
            Severity: InsightSeverity.Warning,
            Title: "Titre",
            Detail: "Détail multi\nligne",
            ActionDeepLink: "https://notion.so/page",
            CreatedAt: T1);

        using (var ctx = fixture.CreateContext())
        {
            var sut = new EfInsightRepository(ctx);
            await sut.StoreSnapshotAsync([source]);
        }

        using (var ctx = fixture.CreateContext())
        {
            var sut = new EfInsightRepository(ctx);
            var latest = await sut.GetLatestAsync();
            latest.Should().ContainSingle().Which.Should().BeEquivalentTo(source);
        }
    }

    private static Insight MakeInsight(
        string id,
        DateTimeOffset createdAt,
        InsightSeverity severity = InsightSeverity.Info,
        string title = "t") =>
        new(
            Id: id,
            RuleId: "r",
            Severity: severity,
            Title: title,
            Detail: "d",
            ActionDeepLink: null,
            CreatedAt: createdAt);
}
