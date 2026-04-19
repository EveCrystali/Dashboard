using Dashboard.Core.Abstractions;
using Dashboard.Data.Persistence.Repositories;

namespace Dashboard.Data.Tests.Persistence;

public sealed class SyncCursorStoreTests
{
    [Fact]
    public async Task Get_retourne_null_quand_absent()
    {
        using var fixture = new SqliteInMemoryFixture();
        using var ctx = fixture.CreateContext();
        var sut = new SyncCursorStore(ctx);

        var cursor = await sut.GetAsync("inconnu");

        cursor.Should().BeNull();
    }

    [Fact]
    public async Task Upsert_insere_puis_met_a_jour()
    {
        using var fixture = new SqliteInMemoryFixture();

        var t1 = new DateTimeOffset(2026, 4, 19, 10, 0, 0, TimeSpan.Zero);
        var t2 = new DateTimeOffset(2026, 4, 19, 11, 0, 0, TimeSpan.Zero);

        using (var ctx = fixture.CreateContext())
        {
            var sut = new SyncCursorStore(ctx);
            await sut.UpsertAsync(new SyncCursor("ds-1", t1, t1));
        }

        using (var ctx = fixture.CreateContext())
        {
            var sut = new SyncCursorStore(ctx);
            await sut.UpsertAsync(new SyncCursor("ds-1", t2, t2));
        }

        using (var ctx = fixture.CreateContext())
        {
            var sut = new SyncCursorStore(ctx);
            var cursor = await sut.GetAsync("ds-1");
            cursor.Should().NotBeNull();
            cursor!.LastEditedSeen.Should().Be(t2);
            cursor.LastSyncCompleted.Should().Be(t2);
        }
    }
}
