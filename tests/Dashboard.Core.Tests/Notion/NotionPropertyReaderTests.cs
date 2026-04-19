using System.Text.Json;
using Dashboard.Core.Notion;

namespace Dashboard.Core.Tests.Notion;

public sealed class NotionPropertyReaderTests
{
    private static readonly NotionPropertyReader Sut = new();

    [Fact]
    public void AsTitle_concatene_les_plain_text()
    {
        var el = Parse("""
        { "type": "title", "title": [
          { "plain_text": "Hello " },
          { "plain_text": "World" }
        ]}
        """);

        Sut.AsTitle(el).Should().Be("Hello World");
    }

    [Fact]
    public void AsTitle_retourne_null_si_tableau_vide()
    {
        var el = Parse("""{ "type": "title", "title": [] }""");

        Sut.AsTitle(el).Should().BeNull();
    }

    [Fact]
    public void AsRichText_concatene_les_plain_text()
    {
        var el = Parse("""
        { "type": "rich_text", "rich_text": [
          { "plain_text": "Un " }, { "plain_text": "deux" }
        ]}
        """);

        Sut.AsRichText(el).Should().Be("Un deux");
    }

    [Fact]
    public void AsDate_retourne_null_si_date_null()
    {
        var el = Parse("""{ "type": "date", "date": null }""");

        Sut.AsDate(el).Should().BeNull();
    }

    [Fact]
    public void AsDate_parse_date_simple_en_UTC_sans_heure()
    {
        var el = Parse("""{ "type": "date", "date": { "start": "2026-04-19", "end": null } }""");

        var range = Sut.AsDate(el);

        range.Should().NotBeNull();
        range!.IsDateTime.Should().BeFalse();
        range.Start.Should().Be(new DateTimeOffset(2026, 4, 19, 0, 0, 0, TimeSpan.Zero));
        range.End.Should().BeNull();
    }

    [Fact]
    public void AsDate_parse_datetime_avec_offset_et_signale_IsDateTime()
    {
        var el = Parse("""{ "type": "date", "date": { "start": "2026-04-19T09:30:00+02:00", "end": null } }""");

        var range = Sut.AsDate(el);

        range.Should().NotBeNull();
        range!.IsDateTime.Should().BeTrue();
        range.Start.Should().Be(new DateTimeOffset(2026, 4, 19, 9, 30, 0, TimeSpan.FromHours(2)));
    }

    [Fact]
    public void AsDate_parse_plage_start_et_end()
    {
        var el = Parse("""{ "type": "date", "date": { "start": "2026-04-19", "end": "2026-04-21" } }""");

        var range = Sut.AsDate(el);

        range.Should().NotBeNull();
        range!.Start.Should().Be(new DateTimeOffset(2026, 4, 19, 0, 0, 0, TimeSpan.Zero));
        range.End.Should().Be(new DateTimeOffset(2026, 4, 21, 0, 0, 0, TimeSpan.Zero));
    }

    [Fact]
    public void AsSelect_retourne_le_nom()
    {
        var el = Parse("""{ "type": "select", "select": { "name": "Haute" } }""");

        Sut.AsSelect(el).Should().Be("Haute");
    }

    [Fact]
    public void AsSelect_retourne_null_si_select_null()
    {
        var el = Parse("""{ "type": "select", "select": null }""");

        Sut.AsSelect(el).Should().BeNull();
    }

    [Fact]
    public void AsStatus_retourne_le_nom()
    {
        var el = Parse("""{ "type": "status", "status": { "name": "🏇 En cours" } }""");

        Sut.AsStatus(el).Should().Be("🏇 En cours");
    }

    [Fact]
    public void AsMultiSelect_retourne_tous_les_noms()
    {
        var el = Parse("""
        { "type": "multi_select", "multi_select": [
          { "name": "A" }, { "name": "B" }
        ]}
        """);

        Sut.AsMultiSelect(el).Should().Equal("A", "B");
    }

    [Fact]
    public void AsMultiSelect_retourne_liste_vide_si_array_vide()
    {
        var el = Parse("""{ "type": "multi_select", "multi_select": [] }""");

        Sut.AsMultiSelect(el).Should().BeEmpty();
    }

    [Fact]
    public void AsNumber_retourne_la_valeur()
    {
        var el = Parse("""{ "type": "number", "number": 42.5 }""");

        Sut.AsNumber(el).Should().Be(42.5);
    }

    [Fact]
    public void AsNumber_retourne_null_si_number_null()
    {
        var el = Parse("""{ "type": "number", "number": null }""");

        Sut.AsNumber(el).Should().BeNull();
    }

    [Fact]
    public void AsCheckbox_retourne_true()
    {
        var el = Parse("""{ "type": "checkbox", "checkbox": true }""");

        Sut.AsCheckbox(el).Should().BeTrue();
    }

    [Fact]
    public void AsCheckbox_retourne_false_par_defaut()
    {
        var el = Parse("""{ "type": "checkbox", "checkbox": false }""");

        Sut.AsCheckbox(el).Should().BeFalse();
    }

    [Fact]
    public void AsRelation_retourne_les_ids()
    {
        var el = Parse("""
        { "type": "relation", "relation": [
          { "id": "u1" }, { "id": "u2" }
        ]}
        """);

        Sut.AsRelation(el).Should().Equal("u1", "u2");
    }

    [Fact]
    public void AsPeople_retourne_les_ids()
    {
        var el = Parse("""
        { "type": "people", "people": [
          { "object": "user", "id": "u1" }
        ]}
        """);

        Sut.AsPeople(el).Should().Equal("u1");
    }

    [Fact]
    public void AsUrl_retourne_la_valeur()
    {
        var el = Parse("""{ "type": "url", "url": "https://example.com" }""");

        Sut.AsUrl(el).Should().Be("https://example.com");
    }

    [Fact]
    public void AsUrl_retourne_null_si_url_null()
    {
        var el = Parse("""{ "type": "url", "url": null }""");

        Sut.AsUrl(el).Should().BeNull();
    }

    [Fact]
    public void AsFiles_retourne_les_noms()
    {
        var el = Parse("""
        { "type": "files", "files": [
          { "name": "cv.pdf", "type": "file" }
        ]}
        """);

        Sut.AsFiles(el).Should().Equal("cv.pdf");
    }

    private static JsonElement Parse(string json) => JsonDocument.Parse(json).RootElement;
}
