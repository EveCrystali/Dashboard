using Android.Content;
using Android.Provider;
using Dashboard.Core.Abstractions.Calendar;

namespace Dashboard.App.Platforms.Android.Services;

/// <summary>
/// Adaptateur fin <c>ICursor</c> &#x2192; POCOs (cf. ADR-0004). Tout l'aspect
/// non testable (ContentResolver, ICursor à index colonnes) est isolé ici ;
/// la logique métier est dans <c>AndroidCalendarService</c> (pure C#).
/// </summary>
/// <remarks>
/// Les noms de colonnes sont passés en littéraux (API publique documentée et
/// stable de <c>CalendarContract</c>) plutôt que via les constantes de binding
/// <c>InterfaceConsts</c>, dont la nomenclature varie entre versions du
/// binding .NET for Android. Les valeurs correspondent aux constantes Java
/// officielles (<c>CALENDAR_DISPLAY_NAME</c>, <c>ALL_DAY</c>, etc.).
/// </remarks>
internal sealed class DefaultCalendarContentReader : ICalendarContentReader
{
    private const string ColId = "_id";
    private const string ColCalendarDisplayName = "calendar_displayName";
    private const string ColAccountName = "account_name";
    private const string ColCalendarColor = "calendar_color";
    private const string ColVisible = "visible";

    private const string ColEventId = "event_id";
    private const string ColCalendarId = "calendar_id";
    private const string ColTitle = "title";
    private const string ColBegin = "begin";
    private const string ColEnd = "end";
    private const string ColAllDay = "allDay";
    private const string ColEventTimezone = "eventTimezone";

    private static readonly string[] CalendarsProjection =
    {
        ColId,
        ColCalendarDisplayName,
        ColAccountName,
        ColCalendarColor,
        ColVisible,
    };

    private static readonly string[] InstancesProjection =
    {
        ColEventId,
        ColCalendarId,
        ColTitle,
        ColBegin,
        ColEnd,
        ColAllDay,
        ColEventTimezone,
    };

    public IEnumerable<RawCalendarRow> ReadCalendars()
    {
        var resolver = global::Android.App.Application.Context.ContentResolver
            ?? throw new InvalidOperationException("ContentResolver indisponible.");
        var uri = CalendarContract.Calendars.ContentUri
            ?? throw new InvalidOperationException("CalendarContract.Calendars.ContentUri null.");

        using var cursor = resolver.Query(uri, CalendarsProjection, $"{ColVisible}=1", null, null);
        if (cursor is null)
        {
            yield break;
        }

        var idIdx = cursor.GetColumnIndexOrThrow(ColId);
        var nameIdx = cursor.GetColumnIndexOrThrow(ColCalendarDisplayName);
        var accountIdx = cursor.GetColumnIndexOrThrow(ColAccountName);
        var colorIdx = cursor.GetColumnIndexOrThrow(ColCalendarColor);
        var visibleIdx = cursor.GetColumnIndexOrThrow(ColVisible);

        while (cursor.MoveToNext())
        {
            yield return new RawCalendarRow(
                CalendarId: cursor.GetLong(idIdx),
                DisplayName: cursor.GetString(nameIdx) ?? string.Empty,
                AccountName: cursor.GetString(accountIdx),
                Color: cursor.GetInt(colorIdx),
                Visible: cursor.GetInt(visibleIdx) == 1);
        }
    }

    public IEnumerable<RawEventRow> ReadInstances(DateTimeOffset from, DateTimeOffset to)
    {
        var resolver = global::Android.App.Application.Context.ContentResolver
            ?? throw new InvalidOperationException("ContentResolver indisponible.");

        var baseUri = CalendarContract.Instances.ContentUri
            ?? throw new InvalidOperationException("CalendarContract.Instances.ContentUri null.");
        var builder = baseUri.BuildUpon()
            ?? throw new InvalidOperationException("Uri.BuildUpon a retourné null.");
        ContentUris.AppendId(builder, from.ToUnixTimeMilliseconds());
        ContentUris.AppendId(builder, to.ToUnixTimeMilliseconds());
        var uri = builder.Build()
            ?? throw new InvalidOperationException("Uri.Build a retourné null.");

        using var cursor = resolver.Query(uri, InstancesProjection, null, null, null);
        if (cursor is null)
        {
            yield break;
        }

        var eventIdIdx = cursor.GetColumnIndexOrThrow(ColEventId);
        var calendarIdIdx = cursor.GetColumnIndexOrThrow(ColCalendarId);
        var titleIdx = cursor.GetColumnIndexOrThrow(ColTitle);
        var beginIdx = cursor.GetColumnIndexOrThrow(ColBegin);
        var endIdx = cursor.GetColumnIndexOrThrow(ColEnd);
        var allDayIdx = cursor.GetColumnIndexOrThrow(ColAllDay);
        var tzIdx = cursor.GetColumnIndexOrThrow(ColEventTimezone);

        while (cursor.MoveToNext())
        {
            yield return new RawEventRow(
                EventId: cursor.GetLong(eventIdIdx),
                CalendarId: cursor.GetLong(calendarIdIdx),
                Title: cursor.GetString(titleIdx),
                BeginUtcMillis: cursor.GetLong(beginIdx),
                EndUtcMillis: cursor.GetLong(endIdx),
                IsAllDay: cursor.GetInt(allDayIdx) == 1,
                EventTimezone: cursor.GetString(tzIdx));
        }
    }
}
