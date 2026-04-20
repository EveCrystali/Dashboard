using Dashboard.Core.Abstractions.Insights;
using Dashboard.Core.Domain;
using Dashboard.Core.Services.Insights;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Dashboard.Core.Tests.Services.Insights;

public sealed class CrossDomainInsightsServiceTests
{
    private static readonly DateTimeOffset Now = new(2026, 4, 20, 10, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Aggrege_les_insights_de_toutes_les_regles_et_stocke_le_snapshot()
    {
        var ruleA = MakeRule("a", [MakeInsight("a-1"), MakeInsight("a-2")]);
        var ruleB = MakeRule("b", [MakeInsight("b-1")]);
        var repo = new Mock<IInsightRepository>();

        var sut = new CrossDomainInsightsService(
            [ruleA.Object, ruleB.Object],
            repo.Object,
            NullLogger<CrossDomainInsightsService>.Instance);

        var insights = await sut.ComputeAndStoreAsync();

        insights.Should().HaveCount(3);
        insights.Select(i => i.Id).Should().Contain(["a-1", "a-2", "b-1"]);
        repo.Verify(r => r.StoreSnapshotAsync(
            It.Is<IEnumerable<Insight>>(list => list.Count() == 3),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Une_regle_en_echec_est_ignoree_sans_bloquer_les_autres()
    {
        var ruleOk = MakeRule("ok", [MakeInsight("ok-1")]);
        var ruleKo = new Mock<IInsightRule>();
        ruleKo.SetupGet(r => r.RuleId).Returns("ko");
        ruleKo.Setup(r => r.EvaluateAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("boom"));
        var repo = new Mock<IInsightRepository>();

        var sut = new CrossDomainInsightsService(
            [ruleKo.Object, ruleOk.Object],
            repo.Object,
            NullLogger<CrossDomainInsightsService>.Instance);

        var insights = await sut.ComputeAndStoreAsync();

        insights.Should().ContainSingle().Which.Id.Should().Be("ok-1");
        repo.Verify(r => r.StoreSnapshotAsync(
            It.IsAny<IEnumerable<Insight>>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task OperationCanceled_remonte_sans_etre_avalee()
    {
        var cts = new CancellationTokenSource();
        var rule = new Mock<IInsightRule>();
        rule.SetupGet(r => r.RuleId).Returns("cancel");
        rule.Setup(r => r.EvaluateAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());
        var repo = new Mock<IInsightRepository>();

        var sut = new CrossDomainInsightsService(
            [rule.Object],
            repo.Object,
            NullLogger<CrossDomainInsightsService>.Instance);

        var act = () => sut.ComputeAndStoreAsync(cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        repo.Verify(r => r.StoreSnapshotAsync(
            It.IsAny<IEnumerable<Insight>>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetLatestAsync_delegue_au_repository()
    {
        var repo = new Mock<IInsightRepository>();
        var expected = new List<Insight> { MakeInsight("latest") };
        repo.Setup(r => r.GetLatestAsync(It.IsAny<CancellationToken>())).ReturnsAsync(expected);

        var sut = new CrossDomainInsightsService(
            [],
            repo.Object,
            NullLogger<CrossDomainInsightsService>.Instance);

        var result = await sut.GetLatestAsync();

        result.Should().BeEquivalentTo(expected);
    }

    private static Mock<IInsightRule> MakeRule(string id, IReadOnlyList<Insight> output)
    {
        var mock = new Mock<IInsightRule>();
        mock.SetupGet(r => r.RuleId).Returns(id);
        mock.Setup(r => r.EvaluateAsync(It.IsAny<CancellationToken>())).ReturnsAsync(output);
        return mock;
    }

    private static Insight MakeInsight(string id) =>
        new(
            Id: id,
            RuleId: "test-rule",
            Severity: InsightSeverity.Info,
            Title: $"insight-{id}",
            Detail: "détail",
            ActionDeepLink: null,
            CreatedAt: Now);
}
