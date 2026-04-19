using Dashboard.Core.Abstractions.Calendar;
using Microsoft.Extensions.DependencyInjection;

namespace Dashboard.Core.Services;

public static class CalendarServiceCollectionExtensions
{
    /// <summary>
    /// Enregistre le service métier de calendrier en <c>Singleton</c>.
    /// L'application doit en complément enregistrer les implémentations
    /// plateforme : <see cref="ICalendarContentReader"/> et
    /// <see cref="ICalendarPermissionRequester"/> (cf. <c>MauiProgram</c>
    /// côté <c>Dashboard.App</c>).
    /// </summary>
    public static IServiceCollection AddCalendarService(this IServiceCollection services)
    {
        services.AddSingleton<ICalendarService, AndroidCalendarService>();
        return services;
    }
}
