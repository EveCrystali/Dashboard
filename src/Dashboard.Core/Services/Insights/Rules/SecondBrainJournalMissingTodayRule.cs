using Dashboard.Core.Abstractions;
using Dashboard.Core.Abstractions.Insights;
using Dashboard.Core.Domain;

namespace Dashboard.Core.Services.Insights.Rules;

/// <summary>
/// Émet un <see cref="InsightSeverity.Info"/> si aucune
/// <see cref="JournalEntry"/> n'a <c>Date.Start</c> égal à la date du jour
/// (selon le fuseau de l'horloge). Sert de rappel doux pour alimenter le
/// Second Brain quotidiennement.
/// </summary>
public sealed class SecondBrainJournalMissingTodayRule : IInsightRule
{
    public const string Id = "second-brain-journal-missing-today";

    private readonly IJournalEntryRepository _journal;
    private readonly IClock _clock;

    public SecondBrainJournalMissingTodayRule(IJournalEntryRepository journal, IClock clock)
    {
        _journal = journal;
        _clock = clock;
    }

    public string RuleId => Id;

    public async Task<IReadOnlyList<Insight>> EvaluateAsync(CancellationToken ct = default)
    {
        var now = _clock.Now;
        var today = now.Date;
        var entries = await _journal.GetAllAsync(ct).ConfigureAwait(false);

        var hasEntryToday = entries.Any(e => e.Date?.Start?.Date == today);
        if (hasEntryToday)
        {
            return Array.Empty<Insight>();
        }

        return
        [
            new Insight(
                Id: Guid.NewGuid().ToString("N"),
                RuleId: Id,
                Severity: InsightSeverity.Info,
                Title: "Pas d'entrée Second Brain aujourd'hui",
                Detail: "Aucune entrée de journal avec la date du jour. Consigner une pensée, décision ou snapshot.",
                ActionDeepLink: null,
                CreatedAt: now),
        ];
    }
}
