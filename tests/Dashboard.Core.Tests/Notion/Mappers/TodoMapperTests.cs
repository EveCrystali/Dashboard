using Dashboard.Core.Domain;
using Dashboard.Core.Notion;
using Dashboard.Core.Notion.Mappers;

namespace Dashboard.Core.Tests.Notion.Mappers;

public sealed class TodoMapperTests
{
    private static readonly NotionPropertyReader Reader = new();

    [Fact]
    public void Map_d_une_tache_complete_remplit_tous_les_champs()
    {
        var page = FixtureLoader.LoadPage("todo-page.json");

        var todo = TodoMapper.Map(page, Reader);

        todo.Id.Should().Be("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        todo.Title.Should().Be("Finir le Lot 5");
        todo.Status.Should().Be(TodoStatus.EnCours);
        todo.Priority.Should().Be(TodoPriority.Haute);
        todo.Tags.Should().Equal(TodoTag.Travail, TodoTag.Apprentissage);
        todo.Agenda.Should().NotBeNull();
        todo.Agenda!.Start.Should().Be(new DateTimeOffset(2026, 4, 19, 0, 0, 0, TimeSpan.Zero));
        todo.Agenda.IsDateTime.Should().BeFalse();
        todo.DueDate.Should().NotBeNull();
        todo.DueDate!.IsDateTime.Should().BeTrue();
        todo.DueDate.Start.Should().Be(new DateTimeOffset(2026, 4, 20, 9, 0, 0, TimeSpan.FromHours(2)));
        todo.AssigneeIds.Should().Equal("user-123");
        todo.AiSummary.Should().Be("T\u00e2che prioritaire avant vendredi.");
        todo.SubtaskUrls.Should().Equal("child-uuid-1", "child-uuid-2");
        todo.ParentUrl.Should().Be("parent-uuid");
    }

    [Fact]
    public void Map_tolere_statut_inconnu_en_fallback_Tache()
    {
        var page = FixtureLoader.LoadPage("todo-page.json");
        var patched = PageWithProperty(page, TodoMapper.ColumnStatus, """
        { "type": "status", "status": { "name": "Inconnu" } }
        """);

        var todo = TodoMapper.Map(patched, Reader);

        todo.Status.Should().Be(TodoStatus.Tache);
    }

    [Fact]
    public void Map_ignore_les_tags_inconnus()
    {
        var page = FixtureLoader.LoadPage("todo-page.json");
        var patched = PageWithProperty(page, TodoMapper.ColumnTags, """
        { "type": "multi_select", "multi_select": [
          { "name": "\u2712 Travail" }, { "name": "Mystere" }
        ]}
        """);

        var todo = TodoMapper.Map(patched, Reader);

        todo.Tags.Should().Equal(TodoTag.Travail);
    }

    private static NotionPage PageWithProperty(NotionPage source, string propertyName, string propertyJson)
    {
        var props = new Dictionary<string, System.Text.Json.JsonElement>(StringComparer.Ordinal);
        foreach (var kv in source.Properties)
        {
            props[kv.Key] = kv.Value;
        }
        props[propertyName] = System.Text.Json.JsonDocument.Parse(propertyJson).RootElement;

        return new NotionPage
        {
            Id = source.Id,
            CreatedTime = source.CreatedTime,
            LastEditedTime = source.LastEditedTime,
            Archived = source.Archived,
            Properties = props,
        };
    }
}
