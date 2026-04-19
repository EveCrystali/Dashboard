using Dashboard.Core.Domain;

namespace Dashboard.Data.Persistence.Entities;

public sealed class TodoEntity
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public TodoStatus Status { get; set; }
    public TodoPriority? Priority { get; set; }
    public string TagsJson { get; set; } = "[]";

    public DateTimeOffset? AgendaStart { get; set; }
    public DateTimeOffset? AgendaEnd { get; set; }
    public bool AgendaIsDateTime { get; set; }

    public DateTimeOffset? DueDateStart { get; set; }
    public DateTimeOffset? DueDateEnd { get; set; }
    public bool DueDateIsDateTime { get; set; }

    public string AssigneeIdsJson { get; set; } = "[]";
    public string? AiSummary { get; set; }
    public string SubtaskUrlsJson { get; set; } = "[]";
    public string? ParentUrl { get; set; }

    public DateTimeOffset LastEditedTime { get; set; }
}
