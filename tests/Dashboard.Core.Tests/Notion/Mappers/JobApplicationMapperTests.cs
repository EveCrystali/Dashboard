using Dashboard.Core.Domain;
using Dashboard.Core.Notion;
using Dashboard.Core.Notion.Mappers;

namespace Dashboard.Core.Tests.Notion.Mappers;

public sealed class JobApplicationMapperTests
{
    private static readonly NotionPropertyReader Reader = new();

    [Fact]
    public void Map_remplit_tous_les_champs_d_une_candidature()
    {
        var page = FixtureLoader.LoadPage("job-application-page.json");

        var app = JobApplicationMapper.Map(page, Reader);

        app.Id.Should().Be("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
        app.Company.Should().Be("Acme Corp");
        app.Status.Should().Be(JobAppStatus.CandidatureEnvoyee);
        app.Positions.Should().Equal(JobPosition.DevCSharpNet, JobPosition.DevFullStackCSharpAngular);
        app.CompanyTypes.Should().Equal(CompanyType.ESN);
        app.Interest.Should().Be(JobInterest.Haute);
        app.ContactMethods.Should().Equal(ContactMethod.LinkedIn, ContactMethod.Mail);
        app.ContactNotes.Should().Be("Jean Dupont, CTO");
        app.ContactDate.Should().NotBeNull();
        app.ContactDate!.Start.Should().Be(new DateTimeOffset(2026, 4, 10, 0, 0, 0, TimeSpan.Zero));
        app.DueDate!.Start.Should().Be(new DateTimeOffset(2026, 4, 25, 0, 0, 0, TimeSpan.Zero));
        app.FollowUpDate.Should().BeNull();
        app.OfferUrl.Should().Be("https://acme.example.com/careers/dev-dotnet");
        app.CvFileIds.Should().Equal("cv-antoine-2026.pdf");
        app.CoverLetterFileIds.Should().BeEmpty();
        app.AiSummary.Should().Be("Poste align\u00e9, r\u00e9mun\u00e9ration \u00e0 v\u00e9rifier.");
    }
}
