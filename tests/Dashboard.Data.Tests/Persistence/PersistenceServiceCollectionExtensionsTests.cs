using Dashboard.Core.Abstractions;
using Dashboard.Data.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Dashboard.Data.Tests.Persistence;

public sealed class PersistenceServiceCollectionExtensionsTests
{
    [Fact]
    public void AddPersistence_enregistre_DbContext_et_repositories()
    {
        var services = new ServiceCollection();

        services.AddPersistence("DataSource=:memory:");

        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();

        scope.ServiceProvider.GetService<AppDbContext>().Should().NotBeNull();
        scope.ServiceProvider.GetService<ITodoRepository>().Should().NotBeNull();
        scope.ServiceProvider.GetService<IJobApplicationRepository>().Should().NotBeNull();
        scope.ServiceProvider.GetService<IJournalEntryRepository>().Should().NotBeNull();
        scope.ServiceProvider.GetService<IHealthReadingRepository>().Should().NotBeNull();
        scope.ServiceProvider.GetService<ISyncCursorStore>().Should().NotBeNull();
    }
}
