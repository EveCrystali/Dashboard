namespace Dashboard.App.Tests;

public sealed class ScaffoldingTests
{
    [Fact]
    public void Solution_compiles_and_test_runner_is_wired()
    {
        const int value = 1 + 1;
        value.Should().Be(2);
    }
}
