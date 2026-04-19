using Dashboard.Core.Domain;
using Dashboard.Data.Persistence.Repositories;

namespace Dashboard.Data.Tests.Persistence;

public sealed class JournalEntryRepositoryTests
{
    [Fact]
    public async Task RoundTrip_conserve_tous_les_champs()
    {
        using var fixture = new SqliteInMemoryFixture();

        var original = new JournalEntry(
            Id: "j-1",
            Title: "Bascule .NET 10",
            Date: new DateRange(new DateTimeOffset(2026, 4, 17, 0, 0, 0, TimeSpan.Zero), null, false),
            Type: JournalType.Decision,
            Domains: [JournalDomain.Emploi, JournalDomain.Transversal],
            Source: JournalSource.Manuel,
            CreatedTime: new DateTimeOffset(2026, 4, 17, 8, 30, 0, TimeSpan.Zero));

        using (var ctx = fixture.CreateContext())
        {
            var repo = new JournalEntryRepository(ctx);
            await repo.UpsertAsync(original, DateTimeOffset.UtcNow);
        }

        using (var ctx = fixture.CreateContext())
        {
            var repo = new JournalEntryRepository(ctx);
            var roundTrip = (await repo.GetAllAsync()).Single();
            roundTrip.Should().BeEquivalentTo(original);
        }
    }
}
