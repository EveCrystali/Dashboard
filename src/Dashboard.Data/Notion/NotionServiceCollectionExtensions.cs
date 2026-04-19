using Dashboard.Core.Notion;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Dashboard.Data.Notion;

public static class NotionServiceCollectionExtensions
{
    /// <summary>
    /// Enregistre le client Notion : options li\u00e9es \u00e0 la section <c>Notion</c>,
    /// <see cref="NotionAuthenticationHandler"/>, HttpClient typ\u00e9 avec
    /// <c>AddStandardResilienceHandler</c> (timeout, retry avec backoff+jitter,
    /// circuit breaker) fourni par <c>Microsoft.Extensions.Http.Resilience</c>.
    /// </summary>
    public static IServiceCollection AddNotionClient(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<NotionOptions>(configuration.GetSection(NotionOptions.SectionName));

        services.AddTransient<NotionAuthenticationHandler>();

        services.AddHttpClient<NotionApiClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<NotionOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseAddress);
        })
        .AddHttpMessageHandler<NotionAuthenticationHandler>()
        .AddStandardResilienceHandler();

        services.AddSingleton<INotionPropertyReader, NotionPropertyReader>();
        services.AddSingleton<NotionService>();

        return services;
    }
}
