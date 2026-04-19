using Dashboard.Core.Domain;
using Dashboard.Data.Persistence.Repositories;

namespace Dashboard.Data.Tests.Persistence;

public sealed class TodoRepositoryTests
{
    [Fact]
    public async Task Upsert_insere_puis_met_a_jour_le_meme_id()
    {
        using var fixture = new SqliteInMemoryFixture();

        using (var ctx = fixture.CreateContext())
        {
            var repo = new TodoRepository(ctx);
            var todo = BuildTodo("t1", "Version 1", TodoStatus.Tache);
            await repo.UpsertAsync(todo, new DateTimeOffset(2026, 4, 19, 10, 0, 0, TimeSpan.Zero));
        }

        using (var ctx = fixture.CreateContext())
        {
            var repo = new TodoRepository(ctx);
            var todo = BuildTodo("t1", "Version 2", TodoStatus.EnCours);
            await repo.UpsertAsync(todo, new DateTimeOffset(2026, 4, 19, 11, 0, 0, TimeSpan.Zero));
        }

        using (var ctx = fixture.CreateContext())
        {
            var repo = new TodoRepository(ctx);
            var all = await repo.GetAllAsync();
            all.Should().ContainSingle();
            all[0].Title.Should().Be("Version 2");
            all[0].Status.Should().Be(TodoStatus.EnCours);
        }
    }

    [Fact]
    public async Task GetAll_round_trip_conserve_tags_dates_et_listes()
    {
        using var fixture = new SqliteInMemoryFixture();

        var original = new TodoItem(
            Id: "t42",
            Title: "Finir lot 6",
            Status: TodoStatus.EnCours,
            Priority: TodoPriority.Haute,
            Tags: [TodoTag.Travail, TodoTag.Apprentissage],
            Agenda: new DateRange(new DateTimeOffset(2026, 4, 19, 0, 0, 0, TimeSpan.Zero), null, false),
            DueDate: new DateRange(new DateTimeOffset(2026, 4, 20, 9, 0, 0, TimeSpan.FromHours(2)), null, true),
            AssigneeIds: ["user-123"],
            AiSummary: "Tâche prioritaire",
            SubtaskUrls: ["child-1", "child-2"],
            ParentUrl: "parent-9");

        using (var ctx = fixture.CreateContext())
        {
            var repo = new TodoRepository(ctx);
            await repo.UpsertAsync(original, DateTimeOffset.UtcNow);
        }

        using (var ctx = fixture.CreateContext())
        {
            var repo = new TodoRepository(ctx);
            var roundTrip = (await repo.GetAllAsync()).Single();
            roundTrip.Should().BeEquivalentTo(original);
        }
    }

    [Fact]
    public async Task DeleteMissing_supprime_les_ids_absents()
    {
        using var fixture = new SqliteInMemoryFixture();

        using (var ctx = fixture.CreateContext())
        {
            var repo = new TodoRepository(ctx);
            await repo.UpsertAsync(BuildTodo("a"), DateTimeOffset.UtcNow);
            await repo.UpsertAsync(BuildTodo("b"), DateTimeOffset.UtcNow);
            await repo.UpsertAsync(BuildTodo("c"), DateTimeOffset.UtcNow);
        }

        using (var ctx = fixture.CreateContext())
        {
            var repo = new TodoRepository(ctx);
            await repo.DeleteMissingAsync(["a", "c"]);
            var ids = (await repo.GetAllAsync()).Select(t => t.Id).OrderBy(x => x).ToList();
            ids.Should().Equal("a", "c");
        }
    }

    [Fact]
    public async Task DeleteMissing_sans_suppression_ne_jette_pas()
    {
        using var fixture = new SqliteInMemoryFixture();
        using var ctx = fixture.CreateContext();
        var repo = new TodoRepository(ctx);
        await repo.UpsertAsync(BuildTodo("x"), DateTimeOffset.UtcNow);

        var act = () => repo.DeleteMissingAsync(["x"]);

        await act.Should().NotThrowAsync();
        (await repo.GetAllAsync()).Should().ContainSingle();
    }

    private static TodoItem BuildTodo(string id, string title = "Test", TodoStatus status = TodoStatus.Tache) =>
        new(
            Id: id,
            Title: title,
            Status: status,
            Priority: null,
            Tags: [],
            Agenda: null,
            DueDate: null,
            AssigneeIds: [],
            AiSummary: null,
            SubtaskUrls: [],
            ParentUrl: null);
}
