using Dashboard.Core.Abstractions;
using Dashboard.Core.Abstractions.Calendar;
using Dashboard.Core.Domain;
using Dashboard.Core.Services.Insights.Rules;
using Moq;

namespace Dashboard.Core.Tests.Services.Insights.Rules;

public sealed class TaskDueTodayWithoutCalendarSlotRuleTests
{
    private static readonly DateTimeOffset Now = new(2026, 4, 20, 7, 30, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset TodayStart = new(2026, 4, 20, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Aucune_tache_due_aujourd_hui_aucun_insight()
    {
        var todos = new Mock<ITodoRepository>();
        todos.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<TodoItem>
            {
                MakeTodo("1", "Demain", due: TodayStart.AddDays(1)),
                MakeTodo("2", "Hier", due: TodayStart.AddDays(-1)),
            });

        var sut = BuildSut(todos.Object, events: []);

        var insights = await sut.EvaluateAsync();

        insights.Should().BeEmpty();
    }

    [Fact]
    public async Task Taches_actives_dues_aujourd_hui_mais_creneau_libre_existe_aucun_insight()
    {
        var todos = new Mock<ITodoRepository>();
        todos.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<TodoItem> { MakeTodo("1", "Urgent", due: TodayStart) });

        var events = new List<CalendarEvent>
        {
            MakeEvent(TodayStart.AddHours(9), TodayStart.AddHours(10)),
            MakeEvent(TodayStart.AddHours(14), TodayStart.AddHours(15)),
        };
        var sut = BuildSut(todos.Object, events);

        var insights = await sut.EvaluateAsync();

        insights.Should().BeEmpty();
    }

    [Fact]
    public async Task Taches_dues_et_aucun_creneau_libre_emet_un_warning_agrege()
    {
        var todos = new Mock<ITodoRepository>();
        todos.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<TodoItem>
            {
                MakeTodo("1", "Rapport", due: TodayStart),
                MakeTodo("2", "Appel client", due: TodayStart),
            });

        var events = new List<CalendarEvent>
        {
            MakeEvent(TodayStart.AddHours(DayStart), TodayStart.AddHours(DayEnd)),
        };
        var sut = BuildSut(todos.Object, events);

        var insights = await sut.EvaluateAsync();

        var single = insights.Should().ContainSingle().Subject;
        single.Severity.Should().Be(InsightSeverity.Warning);
        single.RuleId.Should().Be(TaskDueTodayWithoutCalendarSlotRule.Id);
        single.Title.Should().Contain("2 tâche(s)");
        single.Detail.Should().Contain("Rapport").And.Contain("Appel client");
    }

    [Fact]
    public async Task Les_evenements_all_day_sont_ignores_dans_le_calcul_du_creneau()
    {
        var todos = new Mock<ITodoRepository>();
        todos.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<TodoItem> { MakeTodo("1", "Urgent", due: TodayStart) });

        var events = new List<CalendarEvent>
        {
            new("evt-all", "Férié", TodayStart, TodayStart.AddDays(1), IsAllDay: true, CalendarId: "1", CalendarDisplayName: "Perso"),
        };
        var sut = BuildSut(todos.Object, events);

        var insights = await sut.EvaluateAsync();

        insights.Should().BeEmpty();
    }

    [Fact]
    public async Task Taches_done_ou_annulees_ignorees()
    {
        var todos = new Mock<ITodoRepository>();
        todos.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<TodoItem>
            {
                MakeTodo("1", "Fini", due: TodayStart, status: TodoStatus.Done),
                MakeTodo("2", "Abandonné", due: TodayStart, status: TodoStatus.Annulee),
            });

        var events = new List<CalendarEvent>
        {
            MakeEvent(TodayStart.AddHours(DayStart), TodayStart.AddHours(DayEnd)),
        };
        var sut = BuildSut(todos.Object, events);

        var insights = await sut.EvaluateAsync();

        insights.Should().BeEmpty();
    }

    [Fact]
    public async Task Creneaux_inferieurs_au_seuil_emet_insight()
    {
        var todos = new Mock<ITodoRepository>();
        todos.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<TodoItem> { MakeTodo("1", "Urgent", due: TodayStart) });

        var start = TodayStart.AddHours(DayStart);
        var events = new List<CalendarEvent>
        {
            MakeEvent(start, start.AddMinutes(20)),
            MakeEvent(start.AddMinutes(30), start.AddMinutes(50)),
            MakeEvent(start.AddMinutes(60), TodayStart.AddHours(DayEnd)),
        };
        var sut = BuildSut(todos.Object, events);

        var insights = await sut.EvaluateAsync();

        insights.Should().ContainSingle();
    }

    private const int DayStart = TaskDueTodayWithoutCalendarSlotRule.DayStartHour;
    private const int DayEnd = TaskDueTodayWithoutCalendarSlotRule.DayEndHour;

    private static TaskDueTodayWithoutCalendarSlotRule BuildSut(
        ITodoRepository todos,
        IReadOnlyList<CalendarEvent> events)
    {
        var calendar = new Mock<ICalendarService>();
        calendar
            .Setup(c => c.GetEventsAsync(It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(events);

        var clock = new Mock<IClock>();
        clock.SetupGet(c => c.Now).Returns(Now);

        return new TaskDueTodayWithoutCalendarSlotRule(todos, calendar.Object, clock.Object);
    }

    private static TodoItem MakeTodo(
        string id,
        string title,
        DateTimeOffset due,
        TodoStatus status = TodoStatus.EnCours) =>
        new(
            Id: id,
            Title: title,
            Status: status,
            Priority: null,
            Tags: Array.Empty<TodoTag>(),
            Agenda: null,
            DueDate: new DateRange(due, null, IsDateTime: false),
            AssigneeIds: Array.Empty<string>(),
            AiSummary: null,
            SubtaskUrls: Array.Empty<string>(),
            ParentUrl: null);

    private static CalendarEvent MakeEvent(DateTimeOffset start, DateTimeOffset end) =>
        new(
            Id: Guid.NewGuid().ToString("N"),
            Title: "Event",
            Start: start,
            End: end,
            IsAllDay: false,
            CalendarId: "1",
            CalendarDisplayName: "Perso");
}
