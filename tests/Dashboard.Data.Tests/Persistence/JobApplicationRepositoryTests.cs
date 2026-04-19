using Dashboard.Core.Domain;
using Dashboard.Data.Persistence.Repositories;

namespace Dashboard.Data.Tests.Persistence;

public sealed class JobApplicationRepositoryTests
{
    [Fact]
    public async Task RoundTrip_conserve_les_multi_select_et_les_trois_dates()
    {
        using var fixture = new SqliteInMemoryFixture();

        var original = new JobApplication(
            Id: "job-1",
            Company: "ACME",
            Status: JobAppStatus.Suivi,
            Positions: [JobPosition.DevCSharpNet, JobPosition.DevWeb],
            CompanyTypes: [CompanyType.ESN],
            Interest: JobInterest.Haute,
            ContactMethods: [ContactMethod.LinkedIn, ContactMethod.Mail],
            ContactNotes: "Premier échange.",
            ContactDate: new DateRange(new DateTimeOffset(2026, 4, 10, 0, 0, 0, TimeSpan.Zero), null, false),
            DueDate: new DateRange(new DateTimeOffset(2026, 4, 20, 0, 0, 0, TimeSpan.Zero), null, false),
            FollowUpDate: new DateRange(new DateTimeOffset(2026, 4, 25, 0, 0, 0, TimeSpan.Zero), null, false),
            OfferUrl: "https://example.com/offer",
            CvFileIds: ["cv-1"],
            CoverLetterFileIds: ["lm-1"],
            AiSummary: "Candidature intéressante.");

        using (var ctx = fixture.CreateContext())
        {
            var repo = new JobApplicationRepository(ctx);
            await repo.UpsertAsync(original, DateTimeOffset.UtcNow);
        }

        using (var ctx = fixture.CreateContext())
        {
            var repo = new JobApplicationRepository(ctx);
            var roundTrip = (await repo.GetAllAsync()).Single();
            roundTrip.Should().BeEquivalentTo(original);
        }
    }
}
