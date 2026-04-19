using Android.Content;
using Android.Provider;
using Dashboard.Core.Abstractions.Calendar;

namespace Dashboard.App.Platforms.Android.Services;

/// <summary>
/// Adaptateur fin <c>ICursor</c> &#x2192; POCOs (cf. ADR-0004). Tout l'aspect
/// non testable (ContentResolver, ICursor à index colonnes) est isolé ici ;
/// la logique métier est dans <c>AndroidCalendarService</c> (pure C#).
/// </summary>
internal sealed class DefaultCalendarContentReader : ICalendarContentReader
{
    private static readonly string[] CalendarsProjection =
    {
        BaseColumns.Id!,
        CalendarContract.Calendars.InterfaceConsts.CalendarDisplayName!,
        CalendarContract.Calendars.InterfaceConsts.AccountName!,
        CalendarContract.Calendars.InterfaceConsts.CalendarColor!,
        CalendarContract.Calendars.InterfaceConsts.Visible!,
    };

    private static readonly string[] InstancesProjection =
    {
        CalendarContract.Instances.EventId!,
        CalendarContract.Instances.InterfaceConsts.CalendarId!,
        CalendarContract.Instances.InterfaceConsts.Title!,
        CalendarContract.Instances.Begin!,
        CalendarContract.Instances.End!,
        CalendarContract.Instances.InterfaceConsts.AllDay!,
        CalendarContract.Instances.InterfaceConsts.EventTimezone!,
    };

    public IEnumerable<RawCalendarRow> ReadCalendars()
    {
        var resolver = global::Android.App.Application.Context.ContentResolver
            ?? throw new InvalidOperationException("ContentResolver indisponible.");

        using var cursor = resolver.Query(
            CalendarContract.Calendars.ContentUri!,
            CalendarsProjection,
            $"{CalendarContract.Calendars.InterfaceConsts.Visible}=1",
            null,
            null);
        if (cursor is null)
        {
            yield break;
        }

        var idIdx = cursor.GetColumnIndexOrThrow(BaseColumns.Id!);
        var nameIdx = cursor.GetColumnIndexOrThrow(CalendarContract.Calendars.InterfaceConsts.CalendarDisplayName!);
        var accountIdx = cursor.GetColumnIndexOrThrow(CalendarContract.Calendars.InterfaceConsts.AccountName!);
        var colorIdx = cursor.GetColumnIndexOrThrow(CalendarContract.Calendars.InterfaceConsts.CalendarColor!);
        var visibleIdx = cursor.GetColumnIndexOrThrow(CalendarContract.Calendars.InterfaceConsts.Visible!);

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

        var builder = CalendarContract.Instances.ContentUri!.BuildUpon()
            ?? throw new InvalidOperationException("Impossible de construire l'URI Instances.");
        ContentUris.AppendId(builder, from.ToUnixTimeMilliseconds());
        ContentUris.AppendId(builder, to.ToUnixTimeMilliseconds());
        var uri = builder.Build()
            ?? throw new InvalidOperationException("Impossible de construire l'URI Instances.");

        using var cursor = resolver.Query(uri, InstancesProjection, null, null, null);
        if (cursor is null)
        {
            yield break;
        }

        var eventIdIdx = cursor.GetColumnIndexOrThrow(CalendarContract.Instances.EventId!);
        var calendarIdIdx = cursor.GetColumnIndexOrThrow(CalendarContract.Instances.InterfaceConsts.CalendarId!);
        var titleIdx = cursor.GetColumnIndexOrThrow(CalendarContract.Instances.InterfaceConsts.Title!);
        var beginIdx = cursor.GetColumnIndexOrThrow(CalendarContract.Instances.Begin!);
        var endIdx = cursor.GetColumnIndexOrThrow(CalendarContract.Instances.End!);
        var allDayIdx = cursor.GetColumnIndexOrThrow(CalendarContract.Instances.InterfaceConsts.AllDay!);
        var tzIdx = cursor.GetColumnIndexOrThrow(CalendarContract.Instances.InterfaceConsts.EventTimezone!);

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
