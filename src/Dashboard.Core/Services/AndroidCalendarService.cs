using System.Globalization;
using Dashboard.Core.Abstractions.Calendar;
using Dashboard.Core.Domain;

namespace Dashboard.Core.Services;

/// <summary>
/// Service métier de lecture du calendrier Android. Pure C# : consomme un
/// <see cref="ICalendarContentReader"/> (cf. ADR-0004) qui isole l'API
/// <c>CalendarContract</c>. Vérifie la permission via
/// <see cref="ICalendarPermissionRequester"/> et retourne une liste vide si
/// elle n'est pas accordée. Trie les évènements par <c>Start</c> ascendant.
/// </summary>
public sealed class AndroidCalendarService : ICalendarService
{
    private readonly ICalendarContentReader _reader;
    private readonly ICalendarPermissionRequester _permissions;

    public AndroidCalendarService(
        ICalendarContentReader reader,
        ICalendarPermissionRequester permissions)
    {
        _reader = reader;
        _permissions = permissions;
    }

    public async Task<IReadOnlyList<CalendarEvent>> GetEventsAsync(
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken ct = default)
    {
        if (!await _permissions.IsGrantedAsync(ct).ConfigureAwait(false))
        {
            return Array.Empty<CalendarEvent>();
        }

        var calendars = _reader.ReadCalendars()
            .GroupBy(c => c.CalendarId)
            .ToDictionary(g => g.Key, g => g.First());

        var events = new List<CalendarEvent>();
        foreach (var row in _reader.ReadInstances(from, to))
        {
            var displayName = calendars.TryGetValue(row.CalendarId, out var cal)
                ? cal.DisplayName
                : string.Empty;

            events.Add(new CalendarEvent(
                Id: row.EventId.ToString(CultureInfo.InvariantCulture),
                Title: row.Title ?? string.Empty,
                Start: DateTimeOffset.FromUnixTimeMilliseconds(row.BeginUtcMillis),
                End: DateTimeOffset.FromUnixTimeMilliseconds(row.EndUtcMillis),
                IsAllDay: row.IsAllDay,
                CalendarId: row.CalendarId.ToString(CultureInfo.InvariantCulture),
                CalendarDisplayName: displayName));
        }

        events.Sort(static (a, b) => a.Start.CompareTo(b.Start));
        return events;
    }
}
