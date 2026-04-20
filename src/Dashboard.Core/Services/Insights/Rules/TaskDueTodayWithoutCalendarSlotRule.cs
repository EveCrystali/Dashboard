using Dashboard.Core.Abstractions;
using Dashboard.Core.Abstractions.Calendar;
using Dashboard.Core.Abstractions.Insights;
using Dashboard.Core.Domain;

namespace Dashboard.Core.Services.Insights.Rules;

/// <summary>
/// Émet un <see cref="InsightSeverity.Warning"/> si au moins une tâche
/// est due aujourd'hui (statut actif) ET qu'aucun créneau libre de
/// <see cref="MinFreeSlotMinutes"/> minutes n'existe entre
/// <see cref="DayStartHour"/> et <see cref="DayEndHour"/> (heures locales
/// selon le fuseau de l'horloge). Les événements <c>all-day</c> sont
/// volontairement ignorés : ils correspondent souvent à des rappels ou
/// anniversaires et ne bloquent pas réellement un créneau de travail.
/// </summary>
public sealed class TaskDueTodayWithoutCalendarSlotRule : IInsightRule
{
    public const string Id = "task-due-today-without-calendar-slot";
    public const int DayStartHour = 8;
    public const int DayEndHour = 20;
    public const int MinFreeSlotMinutes = 30;

    private static readonly TimeSpan MinFreeSlot = TimeSpan.FromMinutes(MinFreeSlotMinutes);

    private readonly ITodoRepository _todos;
    private readonly ICalendarService _calendar;
    private readonly IClock _clock;

    public TaskDueTodayWithoutCalendarSlotRule(
        ITodoRepository todos,
        ICalendarService calendar,
        IClock clock)
    {
        _todos = todos;
        _calendar = calendar;
        _clock = clock;
    }

    public string RuleId => Id;

    public async Task<IReadOnlyList<Insight>> EvaluateAsync(CancellationToken ct = default)
    {
        var now = _clock.Now;
        var todayStart = new DateTimeOffset(now.Year, now.Month, now.Day, 0, 0, 0, now.Offset);
        var todayEnd = todayStart.AddDays(1);

        var todos = await _todos.GetAllAsync(ct).ConfigureAwait(false);
        var dueToday = todos
            .Where(IsActive)
            .Where(t => t.DueDate?.Start is { } start && start.Date == todayStart.Date)
            .ToList();

        if (dueToday.Count == 0)
        {
            return Array.Empty<Insight>();
        }

        var events = await _calendar.GetEventsAsync(todayStart, todayEnd, ct).ConfigureAwait(false);

        var freeWindowStart = todayStart.AddHours(DayStartHour);
        var freeWindowEnd = todayStart.AddHours(DayEndHour);

        if (HasFreeSlot(events, freeWindowStart, freeWindowEnd, MinFreeSlot))
        {
            return Array.Empty<Insight>();
        }

        var titles = string.Join(", ", dueToday.Select(t => t.Title));
        var detail = $"Aucun créneau libre ≥ {MinFreeSlotMinutes} min entre "
            + $"{DayStartHour:D2}h et {DayEndHour:D2}h. Tâches : {titles}.";

        return
        [
            new Insight(
                Id: Guid.NewGuid().ToString("N"),
                RuleId: Id,
                Severity: InsightSeverity.Warning,
                Title: $"{dueToday.Count} tâche(s) due(s) aujourd'hui sans créneau libre",
                Detail: detail,
                ActionDeepLink: null,
                CreatedAt: now),
        ];
    }

    private static bool IsActive(TodoItem item) =>
        item.Status != TodoStatus.Done && item.Status != TodoStatus.Annulee;

    private static bool HasFreeSlot(
        IReadOnlyList<CalendarEvent> events,
        DateTimeOffset windowStart,
        DateTimeOffset windowEnd,
        TimeSpan minSlot)
    {
        var busy = events
            .Where(e => !e.IsAllDay)
            .Select(e => (Start: Max(e.Start, windowStart), End: Min(e.End, windowEnd)))
            .Where(i => i.End > i.Start)
            .OrderBy(i => i.Start)
            .ToList();

        var cursor = windowStart;
        foreach (var interval in busy)
        {
            if (interval.Start > cursor && (interval.Start - cursor) >= minSlot)
            {
                return true;
            }
            if (interval.End > cursor)
            {
                cursor = interval.End;
            }
        }

        return windowEnd > cursor && (windowEnd - cursor) >= minSlot;
    }

    private static DateTimeOffset Max(DateTimeOffset a, DateTimeOffset b) => a > b ? a : b;
    private static DateTimeOffset Min(DateTimeOffset a, DateTimeOffset b) => a < b ? a : b;
}
