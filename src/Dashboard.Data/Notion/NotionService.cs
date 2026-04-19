using System.Runtime.CompilerServices;
using Dashboard.Core.Domain;
using Dashboard.Core.Notion;
using Dashboard.Core.Notion.Mappers;
using Microsoft.Extensions.Options;

namespace Dashboard.Data.Notion;

/// <summary>
/// Orchestre les appels de <see cref="NotionApiClient"/> vers les 4 data sources
/// métier et applique les mappers Core pour produire des <see cref="NotionSnapshot{T}"/>.
/// </summary>
public sealed class NotionService : INotionService
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

    public IAsyncEnumerable<NotionSnapshot<TodoItem>> GetTodosAsync(
        DateTimeOffset? editedOnOrAfter = null,
        CancellationToken ct = default) =>
        QueryAndMap(_options.Value.DataSources.Todos, TodoMapper.Map, editedOnOrAfter, ct);

    public IAsyncEnumerable<NotionSnapshot<JobApplication>> GetJobApplicationsAsync(
        DateTimeOffset? editedOnOrAfter = null,
        CancellationToken ct = default) =>
        QueryAndMap(_options.Value.DataSources.JobApplications, JobApplicationMapper.Map, editedOnOrAfter, ct);

    public IAsyncEnumerable<NotionSnapshot<JournalEntry>> GetJournalEntriesAsync(
        DateTimeOffset? editedOnOrAfter = null,
        CancellationToken ct = default) =>
        QueryAndMap(_options.Value.DataSources.Journal, JournalEntryMapper.Map, editedOnOrAfter, ct);

    public IAsyncEnumerable<NotionSnapshot<HealthReading>> GetHealthReadingsAsync(
        DateTimeOffset? editedOnOrAfter = null,
        CancellationToken ct = default) =>
        QueryAndMap(_options.Value.DataSources.Health, HealthReadingMapper.Map, editedOnOrAfter, ct);

    private async IAsyncEnumerable<NotionSnapshot<T>> QueryAndMap<T>(
        string dataSourceId,
        Func<NotionPage, INotionPropertyReader, T> map,
        DateTimeOffset? editedOnOrAfter,
        [EnumeratorCancellation] CancellationToken ct)
    {
        await foreach (var page in _client.QueryDataSourceAsync(dataSourceId, editedOnOrAfter, ct).ConfigureAwait(false))
        {
            yield return new NotionSnapshot<T>(map(page, _reader), page.LastEditedTime);
        }
    }
}
