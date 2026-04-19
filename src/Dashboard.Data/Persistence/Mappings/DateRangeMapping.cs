using Dashboard.Core.Domain;

namespace Dashboard.Data.Persistence.Mappings;

internal static class DateRangeMapping
{
    public static DateRange? FromColumns(DateTimeOffset? start, DateTimeOffset? end, bool isDateTime)
    {
        if (start is null && end is null)
        {
            return null;
        }
        return new DateRange(start, end, isDateTime);
    }

    public static (DateTimeOffset? Start, DateTimeOffset? End, bool IsDateTime) ToColumns(DateRange? range) =>
        range is null
            ? (null, null, false)
            : (range.Start, range.End, range.IsDateTime);
}
