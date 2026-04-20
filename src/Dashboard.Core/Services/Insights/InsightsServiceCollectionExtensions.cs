using Dashboard.Core.Abstractions.Insights;
using Dashboard.Core.Services.Insights.Rules;
using Microsoft.Extensions.DependencyInjection;

namespace Dashboard.Core.Services.Insights;

/// <summary>
/// Enregistre <see cref="ICrossDomainInsightsService"/> et les 4 règles
/// déterministes du Lot 9. L'ordre d'enregistrement est sans importance :
/// <see cref="CrossDomainInsightsService"/> matérialise
/// <c>IEnumerable&lt;IInsightRule&gt;</c> à la construction.
/// </summary>
public static class InsightsServiceCollectionExtensions
{
    public static IServiceCollection AddInsights(this IServiceCollection services)
    {
        services.AddScoped<IInsightRule, TaskDueTodayWithoutCalendarSlotRule>();
        services.AddScoped<IInsightRule, PendingApplicationOver7DaysRule>();
        services.AddScoped<IInsightRule, HealthMonitorStaleOver48hRule>();
        services.AddScoped<IInsightRule, SecondBrainJournalMissingTodayRule>();

        services.AddScoped<ICrossDomainInsightsService, CrossDomainInsightsService>();
        return services;
    }
}
