namespace Dashboard.Core.Domain;

/// <summary>
/// Insight déterministe produit par une <c>IInsightRule</c> et persisté en
/// snapshot (<c>IInsightRepository</c>). Chaque insight référence sa règle
/// d'origine via <see cref="RuleId"/> pour permettre l'analyse historique.
/// </summary>
/// <param name="Id">Identifiant unique de l'insight (GUID string).</param>
/// <param name="RuleId">
/// Identifiant stable de la règle ayant produit l'insight
/// (ex. <c>health-monitor-stale-48h</c>). Sert de clé analytique stable
/// entre snapshots.
/// </param>
/// <param name="Severity">Gravité informative pour l'UI.</param>
/// <param name="Title">Titre court destiné à l'affichage en carte.</param>
/// <param name="Detail">Message complet, peut contenir plusieurs lignes.</param>
/// <param name="ActionDeepLink">
/// Lien optionnel vers la ressource à actionner (URL Notion, deep link
/// interne). <c>null</c> si aucune action directe possible.
/// </param>
/// <param name="CreatedAt">Timestamp UTC d'émission de l'insight.</param>
public sealed record Insight(
    string Id,
    string RuleId,
    InsightSeverity Severity,
    string Title,
    string Detail,
    string? ActionDeepLink,
    DateTimeOffset CreatedAt);
