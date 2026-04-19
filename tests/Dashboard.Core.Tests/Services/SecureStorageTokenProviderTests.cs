using Dashboard.Core.Abstractions;
using Dashboard.Core.Services;
using Moq;

namespace Dashboard.Core.Tests.Services;

public sealed class SecureStorageTokenProviderTests
{
    [Fact]
    public async Task GetNotionTokenAsync_retourne_la_valeur_du_wrapper_quand_presente()
    {
        var wrapper = new Mock<ISecureStorageWrapper>();
        wrapper
            .Setup(w => w.GetAsync(SecureStorageTokenProvider.NotionTokenKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync("valeur-attendue");
        var sut = new SecureStorageTokenProvider(wrapper.Object);

        var token = await sut.GetNotionTokenAsync();

        token.Should().Be("valeur-attendue");
    }

    [Fact]
    public async Task GetNotionTokenAsync_retourne_null_quand_wrapper_vide()
    {
        var wrapper = new Mock<ISecureStorageWrapper>();
        wrapper
            .Setup(w => w.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string?)null);
        var sut = new SecureStorageTokenProvider(wrapper.Object);

        var token = await sut.GetNotionTokenAsync();

        token.Should().BeNull();
    }

    [Fact]
    public async Task SetNotionTokenAsync_persiste_via_le_wrapper_avec_la_bonne_cle()
    {
        var wrapper = new Mock<ISecureStorageWrapper>();
        wrapper
            .Setup(w => w.SetAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        var sut = new SecureStorageTokenProvider(wrapper.Object);

        await sut.SetNotionTokenAsync("nouvelle-valeur");

        wrapper.Verify(
            w => w.SetAsync(SecureStorageTokenProvider.NotionTokenKey, "nouvelle-valeur", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ClearAsync_supprime_la_cle_via_le_wrapper()
    {
        var wrapper = new Mock<ISecureStorageWrapper>();
        wrapper.Setup(w => w.Remove(It.IsAny<string>())).Returns(true);
        var sut = new SecureStorageTokenProvider(wrapper.Object);

        await sut.ClearAsync();

        wrapper.Verify(w => w.Remove(SecureStorageTokenProvider.NotionTokenKey), Times.Once);
    }
}
