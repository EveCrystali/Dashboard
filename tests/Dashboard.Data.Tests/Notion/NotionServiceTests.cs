using System.Net;
using System.Text;
using Dashboard.Core.Notion;
using Dashboard.Data.Notion;
using Microsoft.Extensions.Options;

namespace Dashboard.Data.Tests.Notion;

public sealed class NotionServiceTests
{
    private static readonly NotionOptions NotionOpts = new()
    {
        DataSources = new NotionDataSources
        {
            Todos = "todo-ds",
            JobApplications = "job-ds",
            Journal = "journal-ds",
            Health = "health-ds",
        },
    };

    [Fact]
    public async Task GetTodosAsync_interroge_data_source_Todos_et_map_le_titre()
    {
        var (sut, fake) = BuildSut(_ => JsonResponse("""
        { "results": [ { "id": "p1", "created_time": "2026-04-10T00:00:00Z", "last_edited_time": "2026-04-10T00:00:00Z", "archived": false, "properties": {
            "Nom": { "type": "title", "title": [ { "plain_text": "Ma t\u00e2che" } ] }
          }}],
          "next_cursor": null, "has_more": false }
        """));

        var items = new List<string>();
        await foreach (var snapshot in sut.GetTodosAsync())
        {
            items.Add(snapshot.Item.Title);
        }

        items.Should().Equal("Ma t\u00e2che");
        fake.Requests.Should().ContainSingle()
            .Which.RequestUri!.ToString().Should().EndWith("databases/todo-ds/query");
    }

    [Fact]
    public async Task GetJobApplicationsAsync_cible_le_bon_data_source()
    {
        var (sut, fake) = BuildSut(_ => JsonResponse("""{ "results": [], "next_cursor": null, "has_more": false }"""));

        await foreach (var _ in sut.GetJobApplicationsAsync()) { }

        fake.Requests.Should().ContainSingle()
            .Which.RequestUri!.ToString().Should().EndWith("databases/job-ds/query");
    }

    [Fact]
    public async Task GetJournalEntriesAsync_cible_le_bon_data_source()
    {
        var (sut, fake) = BuildSut(_ => JsonResponse("""{ "results": [], "next_cursor": null, "has_more": false }"""));

        await foreach (var _ in sut.GetJournalEntriesAsync()) { }

        fake.Requests.Should().ContainSingle()
            .Which.RequestUri!.ToString().Should().EndWith("databases/journal-ds/query");
    }

    [Fact]
    public async Task GetHealthReadingsAsync_cible_le_bon_data_source()
    {
        var (sut, fake) = BuildSut(_ => JsonResponse("""{ "results": [], "next_cursor": null, "has_more": false }"""));

        await foreach (var _ in sut.GetHealthReadingsAsync()) { }

        fake.Requests.Should().ContainSingle()
            .Which.RequestUri!.ToString().Should().EndWith("databases/health-ds/query");
    }

    private static (NotionService sut, FakeHttpMessageHandler fake) BuildSut(
        Func<HttpRequestMessage, HttpResponseMessage> responder)
    {
        var fake = new FakeHttpMessageHandler(responder);
        var httpClient = new HttpClient(fake)
        {
            BaseAddress = new Uri("https://example.com/v1/"),
        };
        var apiClient = new NotionApiClient(httpClient);
        var reader = new NotionPropertyReader();
        var service = new NotionService(apiClient, reader, Options.Create(NotionOpts));
        return (service, fake);
    }

    private static HttpResponseMessage JsonResponse(string json) =>
        new(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json"),
        };
}
