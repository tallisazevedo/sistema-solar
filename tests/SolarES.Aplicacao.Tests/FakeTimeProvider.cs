namespace SolarES.Aplicacao.Tests;

internal sealed class FakeTimeProvider(DateTimeOffset agora) : TimeProvider
{
    private DateTimeOffset _agora = agora;

    public override DateTimeOffset GetUtcNow() => _agora;

    public void AvancarPara(DateTimeOffset momento) => _agora = momento;
}
