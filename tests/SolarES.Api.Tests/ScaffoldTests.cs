namespace SolarES.Api.Tests;

public class ScaffoldTests
{
    [Fact]
    public void AssemblyDoApiCarrega()
    {
        var assembly = typeof(Program).Assembly;

        Assert.NotNull(assembly);
    }
}
