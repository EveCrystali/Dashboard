using Dashboard.Core.Domain;

namespace Dashboard.Core.Abstractions.Insights;

/// <summary>
/// Règle déterministe évaluant les données de domaine (Notion, calendrier,
/// etc.) pour émettre zéro ou plusieurs <see cref="Insight"/>. Les règles
/// sont découvertes par DI (<c>IEnumerable&lt;IInsightRule&gt;</c>) et
/// exécutées en parallèle par <c>CrossDomainInsightsService</c>.
/// </summary>
public interface IInsightRule
{
    /// <summary>Identifiant stable de la règle, utilisé comme clé analytique.</summary>
    string RuleId { get; }

    Task<IReadOnlyList<Insight>> EvaluateAsync(CancellationToken ct = default);
}
