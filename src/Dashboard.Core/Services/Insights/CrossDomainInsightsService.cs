using Dashboard.Core.Abstractions.Insights;
using Dashboard.Core.Domain;
using Microsoft.Extensions.Logging;

namespace Dashboard.Core.Services.Insights;

/// <summary>
/// Orchestrateur des règles d'insight. Exécute l'ensemble des
/// <see cref="IInsightRule"/> enregistrées, agrège les résultats en un
/// snapshot, le persiste via <see cref="IInsightRepository"/> et retourne
/// les insights. L'échec d'une règle est isolé (log + skip) pour ne pas
/// masquer les résultats des autres règles.
/// </summary>
public sealed class CrossDomainInsightsService : ICrossDomainInsightsService
{
    private readonly IReadOnlyList<IInsightRule> _rules;
    private readonly IInsightRepository _repository;
    private readonly ILogger<CrossDomainInsightsService> _logger;

    public CrossDomainInsightsService(
        IEnumerable<IInsightRule> rules,
        IInsightRepository repository,
        ILogger<CrossDomainInsightsService> logger)
    {
        _rules = rules.ToList();
        _repository = repository;
        _logger = logger;
    }

    public async Task<IReadOnlyList<Insight>> ComputeAndStoreAsync(CancellationToken ct = default)
    {
        var aggregated = new List<Insight>();

        foreach (var rule in _rules)
        {
            ct.ThrowIfCancellationRequested();

            try
            {
                var insights = await rule.EvaluateAsync(ct).ConfigureAwait(false);
                aggregated.AddRange(insights);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Règle d'insight {RuleId} en échec — ignorée.", rule.RuleId);
            }
        }

        await _repository.StoreSnapshotAsync(aggregated, ct).ConfigureAwait(false);
        return aggregated;
    }

    public Task<IReadOnlyList<Insight>> GetLatestAsync(CancellationToken ct = default) =>
        _repository.GetLatestAsync(ct);
}
