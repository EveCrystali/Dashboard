namespace Dashboard.Core.Abstractions.Calendar;

/// <summary>
/// Abstraction de la permission Android <c>READ_CALENDAR</c>. Découpée en
/// deux opérations : <see cref="IsGrantedAsync"/> est appelée par le service
/// avant chaque lecture (silencieusement) ; <see cref="RequestAsync"/> est
/// déclenchée explicitement par l'UI (settings ou première utilisation).
/// </summary>
public interface ICalendarPermissionRequester
{
    Task<bool> IsGrantedAsync(CancellationToken ct = default);

    Task<bool> RequestAsync(CancellationToken ct = default);
}
