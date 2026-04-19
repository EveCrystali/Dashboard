using Dashboard.Core.Abstractions;
using Dashboard.Core.Services;
using Microsoft.Extensions.Configuration;
using Moq;

namespace Dashboard.Core.Tests.Services;

public sealed class CompositeTokenProviderTests
{
    [Fact]
    public async Task GetNotionTokenAsync_retourne_la_valeur_primaire_quand_presente()
    {
        var primary = BuildPrimary(current: "valeur-primaire");
        var config = BuildConfig(fallback: "valeur-config");
        var sut = new CompositeTokenProvider(primary.Object, config);

        var token = await sut.GetNotionTokenAsync();

        token.Should().Be("valeur-primaire");
    }

    [Fact]
    public async Task GetNotionTokenAsync_tombe_sur_la_configuration_quand_le_primaire_est_vide()
    {
        var primary = BuildPrimary(current: null);
        var config = BuildConfig(fallback: "valeur-config");
        var sut = new CompositeTokenProvider(primary.Object, config);

        var token = await sut.GetNotionTokenAsync();

        token.Should().Be("valeur-config");
    }

    [Fact]
    public async Task GetNotionTokenAsync_tombe_sur_la_configuration_quand_le_primaire_retourne_une_chaine_vide()
    {
        var primary = BuildPrimary(current: string.Empty);
        var config = BuildConfig(fallback: "valeur-config");
        var sut = new CompositeTokenProvider(primary.Object, config);

        var token = await sut.GetNotionTokenAsync();

        token.Should().Be("valeur-config");
    }

    [Fact]
    public async Task GetNotionTokenAsync_retourne_null_quand_primaire_et_configuration_sont_vides()
    {
        var primary = BuildPrimary(current: null);
        var config = BuildConfig(fallback: null);
        var sut = new CompositeTokenProvider(primary.Object, config);

        var token = await sut.GetNotionTokenAsync();

        token.Should().BeNull();
    }

    [Fact]
    public async Task SetNotionTokenAsync_est_delegue_au_primaire()
    {
        var primary = BuildPrimary(current: null);
        var sut = new CompositeTokenProvider(primary.Object, BuildConfig(null));

        await sut.SetNotionTokenAsync("nouvelle-valeur");

        primary.Verify(p => p.SetNotionTokenAsync("nouvelle-valeur", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ClearAsync_est_delegue_au_primaire()
    {
        var primary = BuildPrimary(current: "valeur");
        var sut = new CompositeTokenProvider(primary.Object, BuildConfig(null));

        await sut.ClearAsync();

        primary.Verify(p => p.ClearAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    private static Mock<ITokenProvider> BuildPrimary(string? current)
    {
        var mock = new Mock<ITokenProvider>();
        mock.Setup(p => p.GetNotionTokenAsync(It.IsAny<CancellationToken>())).ReturnsAsync(current);
        mock.Setup(p => p.SetNotionTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        mock.Setup(p => p.ClearAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        return mock;
    }

    private static IConfiguration BuildConfig(string? fallback)
    {
        var mock = new Mock<IConfiguration>();
        mock.Setup(c => c[CompositeTokenProvider.NotionTokenConfigurationKey]).Returns(fallback);
        return mock.Object;
    }
}
