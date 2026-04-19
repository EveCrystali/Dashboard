using Dashboard.Core.Domain;

namespace Dashboard.Core.Abstractions.Calendar;

/// <summary>
/// Façade haut niveau du calendrier. Si la permission
/// <c>READ_CALENDAR</c> n'est pas accordée, retourne une liste vide
/// (silencieusement) afin que les widgets puissent dégrader leur affichage
/// sans gérer eux-mêmes l'absence de permission.
/// </summary>
public interface ICalendarService
{
    Task<IReadOnlyList<CalendarEvent>> GetEventsAsync(
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken ct = default);
}
