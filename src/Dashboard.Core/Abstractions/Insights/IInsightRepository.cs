using Dashboard.Core.Domain;

namespace Dashboard.Core.Abstractions.Insights;

/// <summary>
/// Persistance des snapshots d'insights. Chaque exécution de
/// <c>CrossDomainInsightsService.ComputeAndStoreAsync</c> stocke un snapshot
/// complet (tous les insights générés à un instant t). L'historique permet
/// l'analyse d'évolution et le recalcul paresseux ultérieur.
/// </summary>
public interface IInsightRepository
{
    /// <summary>
    /// Enregistre un snapshot d'insights (sans écraser les précédents).
    /// </summary>
    Task StoreSnapshotAsync(IEnumerable<Insight> insights, CancellationToken ct = default);

    /// <summary>
    /// Retourne les insights du dernier snapshot enregistré, ou une liste
    /// vide si aucun snapshot n'existe.
    /// </summary>
    Task<IReadOnlyList<Insight>> GetLatestAsync(CancellationToken ct = default);

    /// <summary>
    /// Retourne l'historique des insights émis depuis <paramref name="from"/>
    /// (inclusif). Utile pour des analyses transverses (évolution dans le
    /// temps), non utilisé au Lot 9 mais exposé pour éviter un refactor au
    /// Lot 11.
    /// </summary>
    Task<IReadOnlyList<Insight>> GetHistoryAsync(DateTimeOffset from, CancellationToken ct = default);
}
