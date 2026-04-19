using Dashboard.Core.Domain;
using Dashboard.Data.Persistence.Entities;

namespace Dashboard.Data.Persistence.Mappings;

public static class JournalEntryEntityMapping
{
    public static JournalEntry ToDomain(JournalEntryEntity e) =>
        new(
            Id: e.Id,
            Title: e.Title,
            Date: DateRangeMapping.FromColumns(e.DateStart, e.DateEnd, e.DateIsDateTime),
            Type: e.Type,
            Domains: JsonListSerializer.Deserialize<JournalDomain>(e.DomainsJson),
            Source: e.Source,
            CreatedTime: e.CreatedTime);

    public static void CopyInto(JournalEntry item, DateTimeOffset lastEditedTime, JournalEntryEntity target)
    {
        target.Id = item.Id;
        target.Title = item.Title;

        var (dStart, dEnd, dDt) = DateRangeMapping.ToColumns(item.Date);
        target.DateStart = dStart;
        target.DateEnd = dEnd;
        target.DateIsDateTime = dDt;

        target.Type = item.Type;
        target.DomainsJson = JsonListSerializer.Serialize(item.Domains);
        target.Source = item.Source;
        target.CreatedTime = item.CreatedTime;
        target.LastEditedTime = lastEditedTime;
    }
}
