using Dashboard.Core.Domain;

namespace Dashboard.Data.Persistence.Entities;

public sealed class InsightEntity
{
    public string Id { get; set; } = string.Empty;
    public string RuleId { get; set; } = string.Empty;
    public InsightSeverity Severity { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Detail { get; set; } = string.Empty;
    public string? ActionDeepLink { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// Identifiant du snapshot (batch) auquel cet insight appartient. Tous
    /// les insights produits par un même appel à
    /// <c>ComputeAndStoreAsync</c> partagent le même SnapshotId, ce qui
    /// permet de récupérer le dernier snapshot atomiquement et de
    /// reconstituer l'historique.
    /// </summary>
    public string SnapshotId { get; set; } = string.Empty;
}
