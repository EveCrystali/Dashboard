namespace Dashboard.Core.Abstractions.Calendar;

/// <summary>
/// Projection plate d'une ligne de <c>CalendarContract.Calendars</c> exposée
/// par <see cref="ICalendarContentReader"/>. Les types sont alignés sur ceux
/// stockés par Android (identifiants <see langword="long"/>, couleur ARGB en
/// <see langword="int"/>) afin que l'adaptateur reste trivial.
/// </summary>
public sealed record RawCalendarRow(
    long CalendarId,
    string DisplayName,
    string? AccountName,
    int Color,
    bool Visible);
