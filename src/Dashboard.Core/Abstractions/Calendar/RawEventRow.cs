namespace Dashboard.Core.Abstractions.Calendar;

/// <summary>
/// Projection plate d'une ligne de <c>CalendarContract.Instances</c> exposée
/// par <see cref="ICalendarContentReader"/>. <see cref="BeginUtcMillis"/> et
/// <see cref="EndUtcMillis"/> sont des Unix epoch en millisecondes UTC tels
/// que renvoyés par le content provider, y compris pour les évènements
/// <see cref="IsAllDay"/> (Android stocke alors minuit local exprimé en UTC).
/// </summary>
public sealed record RawEventRow(
    long EventId,
    long CalendarId,
    string? Title,
    long BeginUtcMillis,
    long EndUtcMillis,
    bool IsAllDay,
    string? EventTimezone);
