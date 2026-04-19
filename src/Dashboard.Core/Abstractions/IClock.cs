namespace Dashboard.Core.Abstractions;

/// <summary>
/// Abstraction de l'horloge syst\u00e8me pour rendre testable tout code
/// d\u00e9pendant du temps courant (filtrage "aujourd'hui", calculs de d\u00e9lai, etc.).
/// </summary>
public interface IClock
{
    DateTimeOffset Now { get; }
}
