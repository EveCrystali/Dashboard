using System.Net;
using Dashboard.Core.Abstractions;
using Dashboard.Data.Notion;
using Microsoft.Extensions.Options;
using Moq;

namespace Dashboard.Data.Tests.Notion;

public sealed class NotionAuthenticationHandlerTests
{
    [Fact]
    public async Task SendAsync_ajoute_Authorization_Bearer_et_Notion_Version_quand_token_present()
    {
        var captured = await SendAsync(
            token: "valeur-token",
            notionVersion: "2022-06-28");

        captured.Headers.Authorization.Should().NotBeNull();
        captured.Headers.Authorization!.Scheme.Should().Be("Bearer");
        captured.Headers.Authorization!.Parameter.Should().Be("valeur-token");
        captured.Headers.GetValues("Notion-Version").Should().ContainSingle().Which.Should().Be("2022-06-28");
    }

    [Fact]
    public async Task SendAsync_n_ajoute_pas_Authorization_quand_token_vide()
    {
        var captured = await SendAsync(token: null, notionVersion: "2022-06-28");

        captured.Headers.Authorization.Should().BeNull();
        captured.Headers.GetValues("Notion-Version").Should().ContainSingle().Which.Should().Be("2022-06-28");
    }

    [Fact]
    public async Task SendAsync_relit_le_token_a_chaque_requete()
    {
        var tokenProvider = new Mock<ITokenProvider>();
        var sequence = new Queue<string?>(["premier", "second"]);
        tokenProvider
            .Setup(p => p.GetNotionTokenAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => sequence.Dequeue());

        var options = Options.Create(new NotionOptions { NotionVersion = "2022-06-28" });
        var fake = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK));
        var sut = new NotionAuthenticationHandler(tokenProvider.Object, options) { InnerHandler = fake };
        using var client = new HttpClient(sut) { BaseAddress = new Uri("https://example.com/") };

        _ = await client.GetAsync("/a");
        _ = await client.GetAsync("/b");

        fake.Requests[0].Headers.Authorization!.Parameter.Should().Be("premier");
        fake.Requests[1].Headers.Authorization!.Parameter.Should().Be("second");
    }

    private static async Task<HttpRequestMessage> SendAsync(string? token, string notionVersion)
    {
        var tokenProvider = new Mock<ITokenProvider>();
        tokenProvider
            .Setup(p => p.GetNotionTokenAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(token);

        var options = Options.Create(new NotionOptions { NotionVersion = notionVersion });
        var fake = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK));
        var sut = new NotionAuthenticationHandler(tokenProvider.Object, options) { InnerHandler = fake };
        using var client = new HttpClient(sut) { BaseAddress = new Uri("https://example.com/") };

        _ = await client.GetAsync("/any");

        return fake.Requests.Single();
    }
}
