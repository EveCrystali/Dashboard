namespace Dashboard.Core.Domain;

public sealed record JobApplication(
    string Id,
    string Company,
    JobAppStatus Status,
    IReadOnlyList<JobPosition> Positions,
    IReadOnlyList<CompanyType> CompanyTypes,
    JobInterest? Interest,
    IReadOnlyList<ContactMethod> ContactMethods,
    string? ContactNotes,
    DateRange? ContactDate,
    DateRange? DueDate,
    DateRange? FollowUpDate,
    string? OfferUrl,
    IReadOnlyList<string> CvFileIds,
    IReadOnlyList<string> CoverLetterFileIds,
    string? AiSummary);
