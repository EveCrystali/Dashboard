using Dashboard.Core.Abstractions;
using Dashboard.Core.Domain;
using Dashboard.Core.Services.Insights.Rules;
using Moq;

namespace Dashboard.Core.Tests.Services.Insights.Rules;

public sealed class SecondBrainJournalMissingTodayRuleTests
{
    private static readonly DateTimeOffset Now = new(2026, 4, 20, 10, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Aucune_entree_emet_Info()
    {
        var sut = BuildSut([]);

        var insights = await sut.EvaluateAsync();

        var single = insights.Should().ContainSingle().Subject;
        single.Severity.Should().Be(InsightSeverity.Info);
        single.RuleId.Should().Be(SecondBrainJournalMissingTodayRule.Id);
    }

    [Fact]
    public async Task Entree_avec_Date_du_jour_aucun_insight()
    {
        var entry = MakeEntry("e1", date: Now.AddHours(3));
        var sut = BuildSut([entry]);

        var insights = await sut.EvaluateAsync();

        insights.Should().BeEmpty();
    }

    [Fact]
    public async Task Entree_hier_ne_compte_pas_emet_Info()
    {
        var entry = MakeEntry("e1", date: Now.AddDays(-1));
        var sut = BuildSut([entry]);

        var insights = await sut.EvaluateAsync();

        insights.Should().ContainSingle();
    }

    [Fact]
    public async Task Entree_sans_Date_ignoree()
    {
        var entry = MakeEntry("e1", date: null);
        var sut = BuildSut([entry]);

        var insights = await sut.EvaluateAsync();

        insights.Should().ContainSingle();
    }

    private static SecondBrainJournalMissingTodayRule BuildSut(IReadOnlyList<JournalEntry> entries)
    {
        var repo = new Mock<IJournalEntryRepository>();
        repo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(entries);
        var clock = new Mock<IClock>();
        clock.SetupGet(c => c.Now).Returns(Now);
        return new SecondBrainJournalMissingTodayRule(repo.Object, clock.Object);
    }

    private static JournalEntry MakeEntry(string id, DateTimeOffset? date) =>
        new(
            Id: id,
            Title: "Entrée",
            Date: date.HasValue ? new DateRange(date, null, false) : null,
            Type: null,
            Domains: Array.Empty<JournalDomain>(),
            Source: null,
            CreatedTime: DateTimeOffset.MinValue);
}
