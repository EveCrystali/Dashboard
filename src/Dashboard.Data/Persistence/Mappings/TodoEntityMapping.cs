using Dashboard.Core.Domain;
using Dashboard.Data.Persistence.Entities;

namespace Dashboard.Data.Persistence.Mappings;

public static class TodoEntityMapping
{
    public static TodoItem ToDomain(TodoEntity e) =>
        new(
            Id: e.Id,
            Title: e.Title,
            Status: e.Status,
            Priority: e.Priority,
            Tags: JsonListSerializer.Deserialize<TodoTag>(e.TagsJson),
            Agenda: DateRangeMapping.FromColumns(e.AgendaStart, e.AgendaEnd, e.AgendaIsDateTime),
            DueDate: DateRangeMapping.FromColumns(e.DueDateStart, e.DueDateEnd, e.DueDateIsDateTime),
            AssigneeIds: JsonListSerializer.Deserialize<string>(e.AssigneeIdsJson),
            AiSummary: e.AiSummary,
            SubtaskUrls: JsonListSerializer.Deserialize<string>(e.SubtaskUrlsJson),
            ParentUrl: e.ParentUrl);

    public static void CopyInto(TodoItem item, DateTimeOffset lastEditedTime, TodoEntity target)
    {
        target.Id = item.Id;
        target.Title = item.Title;
        target.Status = item.Status;
        target.Priority = item.Priority;
        target.TagsJson = JsonListSerializer.Serialize(item.Tags);

        var (agStart, agEnd, agDt) = DateRangeMapping.ToColumns(item.Agenda);
        target.AgendaStart = agStart;
        target.AgendaEnd = agEnd;
        target.AgendaIsDateTime = agDt;

        var (dueStart, dueEnd, dueDt) = DateRangeMapping.ToColumns(item.DueDate);
        target.DueDateStart = dueStart;
        target.DueDateEnd = dueEnd;
        target.DueDateIsDateTime = dueDt;

        target.AssigneeIdsJson = JsonListSerializer.Serialize(item.AssigneeIds);
        target.AiSummary = item.AiSummary;
        target.SubtaskUrlsJson = JsonListSerializer.Serialize(item.SubtaskUrls);
        target.ParentUrl = item.ParentUrl;
        target.LastEditedTime = lastEditedTime;
    }
}
