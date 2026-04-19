using Dashboard.Core.Abstractions;
using Dashboard.Core.Domain;
using Dashboard.Core.Notion;
using Dashboard.Data.Notion;
using Dashboard.Data.Sync;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;

namespace Dashboard.Data.Tests.Sync;

public sealed class SyncOrchestratorTests
{
    private const string TodoDs = "todo-ds";
    private const string JobDs = "job-ds";
    private const string JournalDs = "journal-ds";
    private const string HealthDs = "health-ds";

    private static readonly NotionOptions Opts = new()
    {
        DataSources = new NotionDataSources
        {
            Todos = TodoDs,
            JobApplications = JobDs,
            Journal = JournalDs,
            Health = HealthDs,
        },
    };

    [Fact]
    public async Task SyncAllAsync_premier_run_lance_full_sync_sans_filtre_et_appelle_DeleteMissing()
    {
        var now = new DateTimeOffset(2026, 4, 18, 10, 0, 0, TimeSpan.Zero);
        var todoEdit = new DateTimeOffset(2026, 4, 17, 9, 30, 0, TimeSpan.Zero);

        var harness = new Harness(now);
        harness.Cursors.Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((SyncCursor?)null);
        harness.Notion.Setup(s => s.GetTodosAsync(null, It.IsAny<CancellationToken>()))
            .Returns(AsyncSeq(new NotionSnapshot<TodoItem>(MakeTodo("t1"), todoEdit)));
        harness.Todos.Setup(r => r.DeleteMissingAsync(It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(3);

        var report = await harness.Build().SyncAllAsync();

        report.AllSucceeded.Should().BeTrue();
        var todoResult = report.Results.Single(r => r.DataSourceId == TodoDs);
        todoResult.WasFullSync.Should().BeTrue();
        todoResult.Upserts.Should().Be(1);
        todoResult.Deletes.Should().Be(3);

        harness.Notion.Verify(s => s.GetTodosAsync(null, It.IsAny<CancellationToken>()), Times.Once);
        harness.Todos.Verify(r => r.UpsertAsync(
            It.Is<TodoItem>(t => t.Id == "t1"), todoEdit, It.IsAny<CancellationToken>()), Times.Once);
        harness.Todos.Verify(r => r.DeleteMissingAsync(
            It.Is<IReadOnlyCollection<string>>(ids => ids.Count == 1 && ids.Contains("t1")),
            It.IsAny<CancellationToken>()), Times.Once);
        harness.Cursors.Verify(c => c.UpsertAsync(
            It.Is<SyncCursor>(s => s.DataSourceId == TodoDs
                                   && s.LastEditedSeen == todoEdit
                                   && s.LastSyncCompleted == now),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SyncAllAsync_dans_la_fenetre_6h_lance_un_delta_avec_filtre_et_sans_DeleteMissing()
    {
        var now = new DateTimeOffset(2026, 4, 18, 10, 0, 0, TimeSpan.Zero);
        var lastSync = now - TimeSpan.FromHours(2);
        var lastSeen = new DateTimeOffset(2026, 4, 17, 18, 0, 0, TimeSpan.Zero);
        var newEdit = new DateTimeOffset(2026, 4, 18, 9, 0, 0, TimeSpan.Zero);

        var harness = new Harness(now);
        harness.Cursors.Setup(c => c.GetAsync(TodoDs, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SyncCursor(TodoDs, lastSeen, lastSync));
        harness.Cursors.Setup(c => c.GetAsync(It.Is<string>(id => id != TodoDs), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string id, CancellationToken _) => new SyncCursor(id, lastSeen, lastSync));
        harness.Notion.Setup(s => s.GetTodosAsync(lastSeen, It.IsAny<CancellationToken>()))
            .Returns(AsyncSeq(new NotionSnapshot<TodoItem>(MakeTodo("t-new"), newEdit)));

        var report = await harness.Build().SyncAllAsync();

        report.AllSucceeded.Should().BeTrue();
        var todoResult = report.Results.Single(r => r.DataSourceId == TodoDs);
        todoResult.WasFullSync.Should().BeFalse();
        todoResult.Upserts.Should().Be(1);
        todoResult.Deletes.Should().Be(0);

        harness.Notion.Verify(s => s.GetTodosAsync(lastSeen, It.IsAny<CancellationToken>()), Times.Once);
        harness.Todos.Verify(r => r.DeleteMissingAsync(
            It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<CancellationToken>()), Times.Never);
        harness.Cursors.Verify(c => c.UpsertAsync(
            It.Is<SyncCursor>(s => s.DataSourceId == TodoDs
                                   && s.LastEditedSeen == newEdit
                                   && s.LastSyncCompleted == lastSync),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SyncAllAsync_apres_6h_relance_un_full_sync()
    {
        var now = new DateTimeOffset(2026, 4, 18, 10, 0, 0, TimeSpan.Zero);
        var lastSync = now - TimeSpan.FromHours(7);
        var lastSeen = now - TimeSpan.FromHours(8);
        var fresh = new DateTimeOffset(2026, 4, 18, 8, 0, 0, TimeSpan.Zero);

        var harness = new Harness(now);
        harness.Cursors.Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string id, CancellationToken _) => new SyncCursor(id, lastSeen, lastSync));
        harness.Notion.Setup(s => s.GetTodosAsync(null, It.IsAny<CancellationToken>()))
            .Returns(AsyncSeq(new NotionSnapshot<TodoItem>(MakeTodo("t1"), fresh)));

        var report = await harness.Build().SyncAllAsync();

        var todoResult = report.Results.Single(r => r.DataSourceId == TodoDs);
        todoResult.WasFullSync.Should().BeTrue();
        harness.Notion.Verify(s => s.GetTodosAsync(null, It.IsAny<CancellationToken>()), Times.Once);
        harness.Todos.Verify(r => r.DeleteMissingAsync(
            It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<CancellationToken>()), Times.Once);
        harness.Cursors.Verify(c => c.UpsertAsync(
            It.Is<SyncCursor>(s => s.DataSourceId == TodoDs && s.LastSyncCompleted == now),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SyncAllAsync_isole_les_erreurs_par_source()
    {
        var now = new DateTimeOffset(2026, 4, 18, 10, 0, 0, TimeSpan.Zero);
        var harness = new Harness(now);
        harness.Cursors.Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((SyncCursor?)null);
        harness.Notion.Setup(s => s.GetJobApplicationsAsync(null, It.IsAny<CancellationToken>()))
            .Returns(ThrowingAsyncSeq<NotionSnapshot<JobApplication>>(new InvalidOperationException("Notion KO")));

        var report = await harness.Build().SyncAllAsync();

        report.AllSucceeded.Should().BeFalse();
        report.Results.Should().HaveCount(4);

        var jobResult = report.Results.Single(r => r.DataSourceId == JobDs);
        jobResult.Success.Should().BeFalse();
        jobResult.ErrorMessage.Should().Be("Notion KO");

        report.Results.Where(r => r.DataSourceId != JobDs).Should().OnlyContain(r => r.Success);
        harness.Cursors.Verify(c => c.UpsertAsync(
            It.Is<SyncCursor>(s => s.DataSourceId == JobDs), It.IsAny<CancellationToken>()), Times.Never);
        harness.Cursors.Verify(c => c.UpsertAsync(
            It.Is<SyncCursor>(s => s.DataSourceId == TodoDs), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SyncAllAsync_AllSucceeded_true_quand_les_4_sources_aboutissent()
    {
        var now = new DateTimeOffset(2026, 4, 18, 10, 0, 0, TimeSpan.Zero);
        var harness = new Harness(now);
        harness.Cursors.Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((SyncCursor?)null);

        var report = await harness.Build().SyncAllAsync();

        report.AllSucceeded.Should().BeTrue();
        report.Results.Select(r => r.DataSourceId).Should().BeEquivalentTo(
            new[] { TodoDs, JobDs, JournalDs, HealthDs });
    }

    [Fact]
    public async Task SyncAllAsync_echec_lecture_curseur_marque_la_source_en_failure()
    {
        var now = new DateTimeOffset(2026, 4, 18, 10, 0, 0, TimeSpan.Zero);
        var harness = new Harness(now);
        harness.Cursors.Setup(c => c.GetAsync(TodoDs, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("DB KO"));
        harness.Cursors.Setup(c => c.GetAsync(It.Is<string>(id => id != TodoDs), It.IsAny<CancellationToken>()))
            .ReturnsAsync((SyncCursor?)null);

        var report = await harness.Build().SyncAllAsync();

        report.AllSucceeded.Should().BeFalse();
        var todoResult = report.Results.Single(r => r.DataSourceId == TodoDs);
        todoResult.Success.Should().BeFalse();
        todoResult.ErrorMessage.Should().Be("DB KO");
        harness.Notion.Verify(s => s.GetTodosAsync(It.IsAny<DateTimeOffset?>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    private sealed class Harness
    {
        public Mock<INotionService> Notion { get; } = new(MockBehavior.Strict);
        public Mock<ITodoRepository> Todos { get; } = new();
        public Mock<IJobApplicationRepository> Jobs { get; } = new();
        public Mock<IJournalEntryRepository> Journal { get; } = new();
        public Mock<IHealthReadingRepository> Health { get; } = new();
        public Mock<ISyncCursorStore> Cursors { get; } = new();
        public Mock<IClock> Clock { get; } = new();

        public Harness(DateTimeOffset now)
        {
            Clock.SetupGet(c => c.Now).Returns(now);

            Notion.Setup(s => s.GetTodosAsync(It.IsAny<DateTimeOffset?>(), It.IsAny<CancellationToken>()))
                .Returns(AsyncSeq<NotionSnapshot<TodoItem>>());
            Notion.Setup(s => s.GetJobApplicationsAsync(It.IsAny<DateTimeOffset?>(), It.IsAny<CancellationToken>()))
                .Returns(AsyncSeq<NotionSnapshot<JobApplication>>());
            Notion.Setup(s => s.GetJournalEntriesAsync(It.IsAny<DateTimeOffset?>(), It.IsAny<CancellationToken>()))
                .Returns(AsyncSeq<NotionSnapshot<JournalEntry>>());
            Notion.Setup(s => s.GetHealthReadingsAsync(It.IsAny<DateTimeOffset?>(), It.IsAny<CancellationToken>()))
                .Returns(AsyncSeq<NotionSnapshot<HealthReading>>());

            Todos.Setup(r => r.DeleteMissingAsync(It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(0);
            Jobs.Setup(r => r.DeleteMissingAsync(It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(0);
            Journal.Setup(r => r.DeleteMissingAsync(It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(0);
            Health.Setup(r => r.DeleteMissingAsync(It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(0);
        }

        public SyncOrchestrator Build() => new(
            Notion.Object,
            Todos.Object,
            Jobs.Object,
            Journal.Object,
            Health.Object,
            Cursors.Object,
            Clock.Object,
            Options.Create(Opts),
            NullLogger<SyncOrchestrator>.Instance);
    }

    private static TodoItem MakeTodo(string id) => new(
        Id: id,
        Title: $"todo-{id}",
        Status: TodoStatus.Tache,
        Priority: null,
        Tags: Array.Empty<TodoTag>(),
        Agenda: null,
        DueDate: null,
        AssigneeIds: Array.Empty<string>(),
        AiSummary: null,
        SubtaskUrls: Array.Empty<string>(),
        ParentUrl: null);

#pragma warning disable CS1998 // async sans await : voulu pour produire IAsyncEnumerable
    private static async IAsyncEnumerable<T> AsyncSeq<T>(params T[] items)
    {
        foreach (var item in items)
        {
            yield return item;
        }
    }

    private static async IAsyncEnumerable<T> ThrowingAsyncSeq<T>(Exception ex)
    {
        throw ex;
#pragma warning disable CS0162 // unreachable : nécessaire pour faire de la méthode un itérateur
        yield break;
#pragma warning restore CS0162
    }
#pragma warning restore CS1998
}
