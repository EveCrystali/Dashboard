using Dashboard.Core.Services;

namespace Dashboard.Core.Tests.Services;

public sealed class SystemClockTests
{
    [Fact]
    public void Now_retourne_une_valeur_proche_de_DateTimeOffset_Now()
    {
        var sut = new SystemClock();

        var reference = DateTimeOffset.Now;
        var now = sut.Now;

        now.Should().BeCloseTo(reference, TimeSpan.FromSeconds(5));
    }
}
