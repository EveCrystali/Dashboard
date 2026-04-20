using Dashboard.Core.Domain;
using Dashboard.Data.Persistence.Entities;

namespace Dashboard.Data.Persistence.Mappings;

public static class InsightEntityMapping
{
    public static Insight ToDomain(InsightEntity e) =>
        new(
            Id: e.Id,
            RuleId: e.RuleId,
            Severity: e.Severity,
            Title: e.Title,
            Detail: e.Detail,
            ActionDeepLink: e.ActionDeepLink,
            CreatedAt: e.CreatedAt);

    public static InsightEntity ToEntity(Insight insight, string snapshotId) =>
        new()
        {
            Id = insight.Id,
            RuleId = insight.RuleId,
            Severity = insight.Severity,
            Title = insight.Title,
            Detail = insight.Detail,
            ActionDeepLink = insight.ActionDeepLink,
            CreatedAt = insight.CreatedAt,
            SnapshotId = snapshotId,
        };
}
