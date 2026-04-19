namespace Dashboard.Data.Tests.Notion;

/// <summary>
/// Handler HTTP factice : enregistre chaque requ\u00eate re\u00e7ue et d\u00e9l\u00e8gue
/// la production de la r\u00e9ponse \u00e0 un callback contr\u00f4l\u00e9 par le test.
/// </summary>
internal sealed class FakeHttpMessageHandler : HttpMessageHandler
{
    private readonly Func<HttpRequestMessage, HttpResponseMessage> _responder;

    public FakeHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> responder)
    {
        _responder = responder;
    }

    public List<HttpRequestMessage> Requests { get; } = [];

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        Requests.Add(request);
        return Task.FromResult(_responder(request));
    }
}
