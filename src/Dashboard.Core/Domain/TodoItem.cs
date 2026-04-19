namespace Dashboard.Core.Domain;

public sealed record TodoItem(
    string Id,
    string Title,
    TodoStatus Status,
    TodoPriority? Priority,
    IReadOnlyList<TodoTag> Tags,
    DateRange? Agenda,
    DateRange? DueDate,
    IReadOnlyList<string> AssigneeIds,
    string? AiSummary,
    IReadOnlyList<string> SubtaskUrls,
    string? ParentUrl);
