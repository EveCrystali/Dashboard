using System.Runtime.CompilerServices;
using Dashboard.Core.Domain;
using Dashboard.Core.Notion;
using Dashboard.Core.Notion.Mappers;
using Microsoft.Extensions.Options;

namespace Dashboard.Data.Notion;

/// <summary>
/// Orchestre les appels de <see cref="NotionApiClient"/> vers les 4 data sources
/// métier et applique les mappers Core pour produire des records de domaine.
/// </summary>
public sealed class NotionService
{
    private readonly NotionApiClient _client;
    private readonly INotionPropertyReader _reader;
    private readonly IOptions<NotionOptions> _options;

    public NotionService(
        NotionApiClient client,
        INotionPropertyReader reader,
        IOptions<NotionOptions> options)
    {
        _client = client;
        _reader = reader;
        _options = options;
    }

    public IAsyncEnumerable<TodoItem> GetTodosAsync(CancellationToken ct = default) =>
        QueryAndMap(_options.Value.DataSources.Todos, TodoMapper.Map, ct);

    public IAsyncEnumerable<JobApplication> GetJobApplicationsAsync(CancellationToken ct = default) =>
        QueryAndMap(_options.Value.DataSources.JobApplications, JobApplicationMapper.Map, ct);

    public IAsyncEnumerable<JournalEntry> GetJournalEntriesAsync(CancellationToken ct = default) =>
        QueryAndMap(_options.Value.DataSources.Journal, JournalEntryMapper.Map, ct);

    public IAsyncEnumerable<HealthReading> GetHealthReadingsAsync(CancellationToken ct = default) =>
        QueryAndMap(_options.Value.DataSources.Health, HealthReadingMapper.Map, ct);

    private async IAsyncEnumerable<T> QueryAndMap<T>(
        string dataSourceId,
        Func<NotionPage, INotionPropertyReader, T> map,
        [EnumeratorCancellation] CancellationToken ct)
    {
        await foreach (var page in _client.QueryDataSourceAsync(dataSourceId, ct).ConfigureAwait(false))
        {
            yield return map(page, _reader);
        }
    }
}
