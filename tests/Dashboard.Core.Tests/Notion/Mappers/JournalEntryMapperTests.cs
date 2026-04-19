using Dashboard.Core.Domain;
using Dashboard.Core.Notion;
using Dashboard.Core.Notion.Mappers;

namespace Dashboard.Core.Tests.Notion.Mappers;

public sealed class JournalEntryMapperTests
{
    private static readonly NotionPropertyReader Reader = new();

    [Fact]
    public void Map_remplit_tous_les_champs_d_une_entree_journal()
    {
        var page = FixtureLoader.LoadPage("journal-entry-page.json");

        var entry = JournalEntryMapper.Map(page, Reader);

        entry.Id.Should().Be("cccccccc-cccc-cccc-cccc-cccccccccccc");
        entry.Title.Should().Be("Bascule vers .NET 10");
        entry.Date.Should().NotBeNull();
        entry.Date!.Start.Should().Be(new DateTimeOffset(2026, 4, 17, 0, 0, 0, TimeSpan.Zero));
        entry.Type.Should().Be(JournalType.Decision);
        entry.Domains.Should().Equal(JournalDomain.Emploi, JournalDomain.Transversal);
        entry.Source.Should().Be(JournalSource.Manuel);
        entry.CreatedTime.Should().Be(new DateTimeOffset(2026, 4, 17, 8, 30, 0, TimeSpan.Zero));
    }
}
