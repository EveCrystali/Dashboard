namespace Dashboard.Core.Abstractions.Calendar;

/// <summary>
/// Abstraction haut niveau du <c>ContentResolver</c> Android pour
/// <c>CalendarContract</c> (cf. ADR-0004). L'implémentation Android lit les
/// curseurs natifs et projette le résultat en POCOs ; le service métier ne
/// dépend ainsi d'aucune API plateforme et reste testable sur <c>net10.0</c>.
/// </summary>
public interface ICalendarContentReader
{
    /// <summary>
    /// Énumère les calendriers visibles connus du device
    /// (<c>CalendarContract.Calendars.Visible = 1</c>).
    /// </summary>
    IEnumerable<RawCalendarRow> ReadCalendars();

    /// <summary>
    /// Énumère les <c>CalendarContract.Instances</c> dans la fenêtre
    /// <paramref name="from"/> &#x2192; <paramref name="to"/> (bornes incluses
    /// côté Android via <c>Instances.CONTENT_URI</c> + <c>begin/end</c>).
    /// </summary>
    IEnumerable<RawEventRow> ReadInstances(DateTimeOffset from, DateTimeOffset to);
}
