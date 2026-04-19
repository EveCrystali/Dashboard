using Dashboard.Core.Domain;

namespace Dashboard.Data.Persistence.Entities;

public sealed class JobApplicationEntity
{
    public string Id { get; set; } = string.Empty;
    public string Company { get; set; } = string.Empty;
    public JobAppStatus Status { get; set; }
    public string PositionsJson { get; set; } = "[]";
    public string CompanyTypesJson { get; set; } = "[]";
    public JobInterest? Interest { get; set; }
    public string ContactMethodsJson { get; set; } = "[]";
    public string? ContactNotes { get; set; }

    public DateTimeOffset? ContactDateStart { get; set; }
    public DateTimeOffset? ContactDateEnd { get; set; }
    public bool ContactDateIsDateTime { get; set; }

    public DateTimeOffset? DueDateStart { get; set; }
    public DateTimeOffset? DueDateEnd { get; set; }
    public bool DueDateIsDateTime { get; set; }

    public DateTimeOffset? FollowUpDateStart { get; set; }
    public DateTimeOffset? FollowUpDateEnd { get; set; }
    public bool FollowUpDateIsDateTime { get; set; }

    public string? OfferUrl { get; set; }
    public string CvFileIdsJson { get; set; } = "[]";
    public string CoverLetterFileIdsJson { get; set; } = "[]";
    public string? AiSummary { get; set; }

    public DateTimeOffset LastEditedTime { get; set; }
}
