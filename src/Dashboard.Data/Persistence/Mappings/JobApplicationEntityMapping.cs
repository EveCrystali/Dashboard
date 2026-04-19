using Dashboard.Core.Domain;
using Dashboard.Data.Persistence.Entities;

namespace Dashboard.Data.Persistence.Mappings;

public static class JobApplicationEntityMapping
{
    public static JobApplication ToDomain(JobApplicationEntity e) =>
        new(
            Id: e.Id,
            Company: e.Company,
            Status: e.Status,
            Positions: JsonListSerializer.Deserialize<JobPosition>(e.PositionsJson),
            CompanyTypes: JsonListSerializer.Deserialize<CompanyType>(e.CompanyTypesJson),
            Interest: e.Interest,
            ContactMethods: JsonListSerializer.Deserialize<ContactMethod>(e.ContactMethodsJson),
            ContactNotes: e.ContactNotes,
            ContactDate: DateRangeMapping.FromColumns(e.ContactDateStart, e.ContactDateEnd, e.ContactDateIsDateTime),
            DueDate: DateRangeMapping.FromColumns(e.DueDateStart, e.DueDateEnd, e.DueDateIsDateTime),
            FollowUpDate: DateRangeMapping.FromColumns(e.FollowUpDateStart, e.FollowUpDateEnd, e.FollowUpDateIsDateTime),
            OfferUrl: e.OfferUrl,
            CvFileIds: JsonListSerializer.Deserialize<string>(e.CvFileIdsJson),
            CoverLetterFileIds: JsonListSerializer.Deserialize<string>(e.CoverLetterFileIdsJson),
            AiSummary: e.AiSummary);

    public static void CopyInto(JobApplication item, DateTimeOffset lastEditedTime, JobApplicationEntity target)
    {
        target.Id = item.Id;
        target.Company = item.Company;
        target.Status = item.Status;
        target.PositionsJson = JsonListSerializer.Serialize(item.Positions);
        target.CompanyTypesJson = JsonListSerializer.Serialize(item.CompanyTypes);
        target.Interest = item.Interest;
        target.ContactMethodsJson = JsonListSerializer.Serialize(item.ContactMethods);
        target.ContactNotes = item.ContactNotes;

        var (cStart, cEnd, cDt) = DateRangeMapping.ToColumns(item.ContactDate);
        target.ContactDateStart = cStart;
        target.ContactDateEnd = cEnd;
        target.ContactDateIsDateTime = cDt;

        var (dStart, dEnd, dDt) = DateRangeMapping.ToColumns(item.DueDate);
        target.DueDateStart = dStart;
        target.DueDateEnd = dEnd;
        target.DueDateIsDateTime = dDt;

        var (fStart, fEnd, fDt) = DateRangeMapping.ToColumns(item.FollowUpDate);
        target.FollowUpDateStart = fStart;
        target.FollowUpDateEnd = fEnd;
        target.FollowUpDateIsDateTime = fDt;

        target.OfferUrl = item.OfferUrl;
        target.CvFileIdsJson = JsonListSerializer.Serialize(item.CvFileIds);
        target.CoverLetterFileIdsJson = JsonListSerializer.Serialize(item.CoverLetterFileIds);
        target.AiSummary = item.AiSummary;
        target.LastEditedTime = lastEditedTime;
    }
}
