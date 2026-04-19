using Dashboard.Core.Domain;

namespace Dashboard.Core.Notion;

/// <summary>
/// Façade haut niveau au-dessus des appels Notion typés. Toutes les méthodes
/// supportent un filtre incrémental optionnel <paramref name="editedOnOrAfter"/>
/// transmis à Notion via <c>filter.last_edited_time.on_or_after</c>.
/// Chaque résultat est un <see cref="NotionSnapshot{T}"/> exposant l'item de
/// domaine et le <c>last_edited_time</c> nécessaire à l'orchestrateur de sync.
/// </summary>
public interface INotionService
{
    IAsyncEnumerable<NotionSnapshot<TodoItem>> GetTodosAsync(
        DateTimeOffset? editedOnOrAfter = null,
        CancellationToken ct = default);

    IAsyncEnumerable<NotionSnapshot<JobApplication>> GetJobApplicationsAsync(
        DateTimeOffset? editedOnOrAfter = null,
        CancellationToken ct = default);

    IAsyncEnumerable<NotionSnapshot<JournalEntry>> GetJournalEntriesAsync(
        DateTimeOffset? editedOnOrAfter = null,
        CancellationToken ct = default);

    IAsyncEnumerable<NotionSnapshot<HealthReading>> GetHealthReadingsAsync(
        DateTimeOffset? editedOnOrAfter = null,
        CancellationToken ct = default);
}
