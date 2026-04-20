using Dashboard.Core.Abstractions;
using Dashboard.Core.Abstractions.Insights;
using Dashboard.Core.Domain;

namespace Dashboard.Core.Services.Insights.Rules;

/// <summary>
/// Émet un <see cref="InsightSeverity.Info"/> par candidature en statut
/// <see cref="JobAppStatus.Suivi"/> qui stagne : soit sa
/// <c>FollowUpDate</c> explicite est dépassée, soit — si aucune date de
/// relance n'est définie — son premier contact remonte à plus de
/// <see cref="StaleThresholdDays"/> jours.
/// </summary>
public sealed class PendingApplicationOver7DaysRule : IInsightRule
{
    public const string Id = "pending-application-over-7-days";
    public const int StaleThresholdDays = 7;

    private static readonly TimeSpan StaleThreshold = TimeSpan.FromDays(StaleThresholdDays);

    private readonly IJobApplicationRepository _applications;
    private readonly IClock _clock;

    public PendingApplicationOver7DaysRule(IJobApplicationRepository applications, IClock clock)
    {
        _applications = applications;
        _clock = clock;
    }

    public string RuleId => Id;

    public async Task<IReadOnlyList<Insight>> EvaluateAsync(CancellationToken ct = default)
    {
        var now = _clock.Now;
        var applications = await _applications.GetAllAsync(ct).ConfigureAwait(false);

        var results = new List<Insight>();
        foreach (var app in applications)
        {
            if (app.Status != JobAppStatus.Suivi)
            {
                continue;
            }

            if (!TryGetStaleReference(app, now, out var reference, out var reason))
            {
                continue;
            }

            var daysSince = (int)(now - reference).TotalDays;
            results.Add(new Insight(
                Id: Guid.NewGuid().ToString("N"),
                RuleId: Id,
                Severity: InsightSeverity.Info,
                Title: $"Relancer {app.Company}",
                Detail: $"Candidature en suivi — {reason} ({daysSince} j).",
                ActionDeepLink: app.OfferUrl,
                CreatedAt: now));
        }

        return results;
    }

    private static bool TryGetStaleReference(
        JobApplication app,
        DateTimeOffset now,
        out DateTimeOffset reference,
        out string reason)
    {
        if (app.FollowUpDate?.Start is { } followUp && followUp < now)
        {
            reference = followUp;
            reason = "date de relance dépassée";
            return true;
        }

        if (app.FollowUpDate?.Start is null
            && app.ContactDate?.Start is { } contact
            && (now - contact) > StaleThreshold)
        {
            reference = contact;
            reason = "dernier contact";
            return true;
        }

        reference = default;
        reason = string.Empty;
        return false;
    }
}
