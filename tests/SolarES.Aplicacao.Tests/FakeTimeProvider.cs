namespace SolarES.Aplicacao.Tests;

internal sealed class FakeTimeProvider(DateTimeOffset agora) : TimeProvider
{
    public override DateTimeOffset GetUtcNow() => agora;
}
