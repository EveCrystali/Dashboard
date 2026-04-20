namespace Dashboard.Core.Domain;

/// <summary>
/// Gravité d'un <see cref="Insight"/>. La représentation visuelle associée
/// (icône + couleur) est définie au niveau UI (Lot 10) et respecte la
/// contrainte d'accessibilité daltonienne du projet : tout signal coloré
/// est doublé d'une icône ou d'un label texte.
/// </summary>
public enum InsightSeverity
{
    Info = 0,
    Warning = 1,
    Critical = 2,
}
