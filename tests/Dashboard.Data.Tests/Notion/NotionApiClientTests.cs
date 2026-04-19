using System.Net;
using System.Text;
using Dashboard.Core.Notion;
using Dashboard.Data.Notion;

namespace Dashboard.Data.Tests.Notion;

public sealed class NotionApiClientTests
{
    private const string DataSourceId = "ddd8f621-4493-465a-80c4-657d461c04b0";

    [Fact]
    public async Task QueryDataSourceOneBatchAsync_poste_sans_cursor_et_deserialise_la_reponse()
    {
        var fake = new FakeHttpMessageHandler(_ => JsonResponse("""
        {
          "results": [
            { "id": "page-1", "created_time": "2026-04-01T10:00:00Z", "last_edited_time": "2026-04-02T10:00:00Z", "archived": false, "properties": {} }
          ],
          "next_cursor": null,
          "has_more": false
        }
        """));
        var sut = BuildClient(fake);

        var response = await sut.QueryDataSourceOneBatchAsync(DataSourceId, startCursor: null);

        fake.Requests.Should().ContainSingle();
        var request = fake.Requests[0];
        request.Method.Should().Be(HttpMethod.Post);
        request.RequestUri!.ToString().Should().Be($"https://example.com/v1/databases/{DataSourceId}/query");
        var body = await request.Content!.ReadAsStringAsync();
        body.Should().NotContain("start_cursor");

        response.Results.Should().ContainSingle().Which.Id.Should().Be("page-1");
        response.HasMore.Should().BeFalse();
        response.NextCursor.Should().BeNull();
    }

    [Fact]
    public async Task QueryDataSourceOneBatchAsync_inclut_start_cursor_quand_fourni()
    {
        var fake = new FakeHttpMessageHandler(_ => JsonResponse("""{ "results": [], "next_cursor": null, "has_more": false }"""));
        var sut = BuildClient(fake);

        _ = await sut.QueryDataSourceOneBatchAsync(DataSourceId, startCursor: "curseur-42");

        var body = await fake.Requests[0].Content!.ReadAsStringAsync();
        body.Should().Contain("\"start_cursor\"").And.Contain("curseur-42");
    }

    [Fact]
    public async Task QueryDataSourceAsync_itere_tous_les_batches_jusqu_a_has_more_false()
    {
        var responses = new Queue<HttpResponseMessage>();
        responses.Enqueue(JsonResponse("""
        { "results": [{ "id": "p1", "created_time": "2026-04-01T10:00:00Z", "last_edited_time": "2026-04-01T10:00:00Z", "archived": false, "properties": {} }],
          "next_cursor": "c1", "has_more": true }
        """));
        responses.Enqueue(JsonResponse("""
        { "results": [{ "id": "p2", "created_time": "2026-04-02T10:00:00Z", "last_edited_time": "2026-04-02T10:00:00Z", "archived": false, "properties": {} }],
          "next_cursor": "c2", "has_more": true }
        """));
        responses.Enqueue(JsonResponse("""
        { "results": [{ "id": "p3", "created_time": "2026-04-03T10:00:00Z", "last_edited_time": "2026-04-03T10:00:00Z", "archived": false, "properties": {} }],
          "next_cursor": null, "has_more": false }
        """));

        var fake = new FakeHttpMessageHandler(_ => responses.Dequeue());
        var sut = BuildClient(fake);

        var ids = new List<string>();
        await foreach (var page in sut.QueryDataSourceAsync(DataSourceId))
        {
            ids.Add(page.Id);
        }

        ids.Should().Equal("p1", "p2", "p3");
        fake.Requests.Should().HaveCount(3);
        (await fake.Requests[0].Content!.ReadAsStringAsync()).Should().NotContain("start_cursor");
        (await fake.Requests[1].Content!.ReadAsStringAsync()).Should().Contain("c1");
        (await fake.Requests[2].Content!.ReadAsStringAsync()).Should().Contain("c2");
    }

    [Fact]
    public async Task QueryDataSourceOneBatchAsync_jette_si_code_http_non_reussi()
    {
        var fake = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.Unauthorized)
        {
            Content = new StringContent("{}", Encoding.UTF8, "application/json")
        });
        var sut = BuildClient(fake);

        var act = () => sut.QueryDataSourceOneBatchAsync(DataSourceId, startCursor: null);

        await act.Should().ThrowAsync<HttpRequestException>();
    }

    private static NotionApiClient BuildClient(FakeHttpMessageHandler handler)
    {
        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://example.com/v1/"),
        };
        return new NotionApiClient(httpClient);
    }

    private static HttpResponseMessage JsonResponse(string json) =>
        new(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json"),
        };
}
