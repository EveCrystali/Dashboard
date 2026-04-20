using Dashboard.Core.Abstractions;
using Dashboard.Core.Domain;
using Dashboard.Core.Services.Insights.Rules;
using Moq;

namespace Dashboard.Core.Tests.Services.Insights.Rules;

public sealed class PendingApplicationOver7DaysRuleTests
{
    private static readonly DateTimeOffset Now = new(2026, 4, 20, 10, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Aucune_candidature_aucun_insight()
    {
        var sut = BuildSut([]);

        var insights = await sut.EvaluateAsync();

        insights.Should().BeEmpty();
    }

    [Fact]
    public async Task Candidature_avec_FollowUpDate_depassee_emet_un_Info_avec_deep_link()
    {
        var app = MakeApp(
            id: "a1",
            company: "Acme",
            status: JobAppStatus.Suivi,
            followUp: Now.AddDays(-2),
            offerUrl: "https://example.com/offer-a1");
        var sut = BuildSut([app]);

        var insights = await sut.EvaluateAsync();

        var single = insights.Should().ContainSingle().Subject;
        single.Severity.Should().Be(InsightSeverity.Info);
        single.RuleId.Should().Be(PendingApplicationOver7DaysRule.Id);
        single.Title.Should().Contain("Acme");
        single.ActionDeepLink.Should().Be("https://example.com/offer-a1");
    }

    [Fact]
    public async Task Candidature_sans_FollowUpDate_mais_ContactDate_au_dela_de_7j_emet_insight()
    {
        var app = MakeApp(
            id: "a1",
            company: "Beta",
            status: JobAppStatus.Suivi,
            contactDate: Now.AddDays(-10));
        var sut = BuildSut([app]);

        var insights = await sut.EvaluateAsync();

        insights.Should().ContainSingle();
    }

    [Fact]
    public async Task Candidature_ContactDate_moins_de_7j_aucun_insight()
    {
        var app = MakeApp(
            id: "a1",
            company: "Gamma",
            status: JobAppStatus.Suivi,
            contactDate: Now.AddDays(-3));
        var sut = BuildSut([app]);

        var insights = await sut.EvaluateAsync();

        insights.Should().BeEmpty();
    }

    [Fact]
    public async Task Candidature_pas_en_Suivi_ignoree_meme_si_stale()
    {
        var app = MakeApp(
            id: "a1",
            company: "Delta",
            status: JobAppStatus.Archive,
            contactDate: Now.AddDays(-30));
        var sut = BuildSut([app]);

        var insights = await sut.EvaluateAsync();

        insights.Should().BeEmpty();
    }

    [Fact]
    public async Task FollupUpDate_future_non_depassee_aucun_insight_meme_si_contact_ancien()
    {
        var app = MakeApp(
            id: "a1",
            company: "Epsilon",
            status: JobAppStatus.Suivi,
            followUp: Now.AddDays(3),
            contactDate: Now.AddDays(-30));
        var sut = BuildSut([app]);

        var insights = await sut.EvaluateAsync();

        insights.Should().BeEmpty();
    }

    private static PendingApplicationOver7DaysRule BuildSut(IReadOnlyList<JobApplication> apps)
    {
        var repo = new Mock<IJobApplicationRepository>();
        repo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(apps);
        var clock = new Mock<IClock>();
        clock.SetupGet(c => c.Now).Returns(Now);
        return new PendingApplicationOver7DaysRule(repo.Object, clock.Object);
    }

    private static JobApplication MakeApp(
        string id,
        string company,
        JobAppStatus status,
        DateTimeOffset? followUp = null,
        DateTimeOffset? contactDate = null,
        string? offerUrl = null) =>
        new(
            Id: id,
            Company: company,
            Status: status,
            Positions: Array.Empty<JobPosition>(),
            CompanyTypes: Array.Empty<CompanyType>(),
            Interest: null,
            ContactMethods: Array.Empty<ContactMethod>(),
            ContactNotes: null,
            ContactDate: contactDate.HasValue ? new DateRange(contactDate, null, false) : null,
            DueDate: null,
            FollowUpDate: followUp.HasValue ? new DateRange(followUp, null, false) : null,
            OfferUrl: offerUrl,
            CvFileIds: Array.Empty<string>(),
            CoverLetterFileIds: Array.Empty<string>(),
            AiSummary: null);
}
