using Dashboard.Core.Abstractions;
using Dashboard.Core.Abstractions.Insights;
using Dashboard.Core.Domain;

namespace Dashboard.Core.Services.Insights.Rules;

/// <summary>
/// Émet un <see cref="InsightSeverity.Critical"/> si la dernière donnée de
/// <see cref="HealthReading"/> disponible est antérieure à
/// <see cref="StaleThresholdHours"/> heures, ou si aucune donnée n'est
/// enregistrée. Utilise <c>Date.Start</c> si renseigné, sinon
/// <c>CreatedTime</c>.
/// </summary>
public sealed class HealthMonitorStaleOver48hRule : IInsightRule
{
    public const string Id = "health-monitor-stale-48h";
    public const int StaleThresholdHours = 48;

    private static readonly TimeSpan StaleThreshold = TimeSpan.FromHours(StaleThresholdHours);

    private readonly IHealthReadingRepository _health;
    private readonly IClock _clock;

    public HealthMonitorStaleOver48hRule(IHealthReadingRepository health, IClock clock)
    {
        _health = health;
        _clock = clock;
    }

    public string RuleId => Id;

    public async Task<IReadOnlyList<Insight>> EvaluateAsync(CancellationToken ct = default)
    {
        var now = _clock.Now;
        var readings = await _health.GetAllAsync(ct).ConfigureAwait(false);

        if (readings.Count == 0)
        {
            return
            [
                new Insight(
                    Id: Guid.NewGuid().ToString("N"),
                    RuleId: Id,
                    Severity: InsightSeverity.Critical,
                    Title: "Health Monitor jamais renseigné",
                    Detail: "Aucune donnée de santé enregistrée. Saisir une entrée dans Notion.",
                    ActionDeepLink: null,
                    CreatedAt: now),
            ];
        }

        var latest = readings.Max(r => r.Date?.Start ?? r.CreatedTime);
        var gap = now - latest;
        if (gap <= StaleThreshold)
        {
            return Array.Empty<Insight>();
        }

        var hours = (int)gap.TotalHours;
        return
        [
            new Insight(
                Id: Guid.NewGuid().ToString("N"),
                RuleId: Id,
                Severity: InsightSeverity.Critical,
                Title: $"Health Monitor silencieux depuis {hours} h",
                Detail: $"Dernière donnée santé il y a {hours} h (seuil {StaleThresholdHours} h).",
                ActionDeepLink: null,
                CreatedAt: now),
        ];
    }
}
