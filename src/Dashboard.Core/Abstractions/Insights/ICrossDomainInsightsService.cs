using Dashboard.Core.Domain;

namespace Dashboard.Core.Abstractions.Insights;

/// <summary>
/// Orchestrateur des <see cref="IInsightRule"/>. Exécute l'ensemble des
/// règles enregistrées, persiste le snapshot agrégé et expose les insights
/// du dernier snapshot pour l'UI.
/// </summary>
public interface ICrossDomainInsightsService
{
    /// <summary>
    /// Exécute toutes les règles, agrège les insights, stocke le snapshot
    /// et retourne les insights produits.
    /// </summary>
    Task<IReadOnlyList<Insight>> ComputeAndStoreAsync(CancellationToken ct = default);

    /// <summary>
    /// Retourne le dernier snapshot persisté (sans ré-évaluation).
    /// </summary>
    Task<IReadOnlyList<Insight>> GetLatestAsync(CancellationToken ct = default);
}
