using Dashboard.Core.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Dashboard.Data.Sync;

public static class SyncServiceCollectionExtensions
{
    /// <summary>
    /// Enregistre l'orchestrateur de synchronisation. Les dépendances
    /// (<see cref="Dashboard.Core.Notion.INotionService"/>, repositories,
    /// <see cref="ISyncCursorStore"/>, <see cref="IClock"/>) doivent être
    /// enregistrées séparément via <c>AddNotionClient</c>, <c>AddPersistence</c>
    /// et l'horloge applicative.
    /// </summary>
    public static IServiceCollection AddSyncOrchestrator(this IServiceCollection services)
    {
        services.AddScoped<ISyncOrchestrator, SyncOrchestrator>();
        return services;
    }
}
