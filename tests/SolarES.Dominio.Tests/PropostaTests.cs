using SolarES.Dominio;
using SolarES.Dominio.Proposta;
using PropostaEntidade = SolarES.Dominio.Proposta.Proposta;

namespace SolarES.Dominio.Tests;

public sealed class PropostaTests
{
    private static PropostaEntidade CriarEmitida(DateTimeOffset validaAte) => new()
    {
        Id = Guid.NewGuid(),
        SimulacaoId = Guid.NewGuid(),
        Numero = "PROP-2026-0001",
        ConfiguracaoVersaoId = Guid.NewGuid(),
        ValidaAte = validaAte,
        Status = StatusProposta.Emitida,
        CriadoEm = validaAte.AddDays(-15),
        AtualizadoEm = validaAte.AddDays(-15),
    };

    [Fact]
    public void Dada_PropostaEmitidaDentroDoPrazo_Quando_Aceita_Entao_MudaParaAceitaEPreencheAceitaEm()
    {
        var validaAte = new DateTimeOffset(2026, 10, 1, 0, 0, 0, TimeSpan.Zero);
        var proposta = CriarEmitida(validaAte);
        var agora = validaAte.AddDays(-1);

        proposta.Aceitar(agora);

        Assert.Equal(StatusProposta.Aceita, proposta.Status);
        Assert.Equal(agora, proposta.AceitaEm);
    }

    [Fact]
    public void Dada_PropostaNoInstanteExatoDeValidaAte_Quando_Aceita_Entao_Permite()
    {
        var validaAte = new DateTimeOffset(2026, 10, 1, 0, 0, 0, TimeSpan.Zero);
        var proposta = CriarEmitida(validaAte);

        proposta.Aceitar(validaAte);

        Assert.Equal(StatusProposta.Aceita, proposta.Status);
    }

    [Fact]
    public void Dada_PropostaVencida_Quando_Aceita_Entao_Recusa()
    {
        var validaAte = new DateTimeOffset(2026, 10, 1, 0, 0, 0, TimeSpan.Zero);
        var proposta = CriarEmitida(validaAte);

        Assert.Throws<TransicaoInvalidaException>(() => proposta.Aceitar(validaAte.AddSeconds(1)));
        Assert.Equal(StatusProposta.Emitida, proposta.Status);
    }

    [Fact]
    public void Dada_PropostaJaAceita_Quando_AceitaNovamente_Entao_Recusa()
    {
        var validaAte = new DateTimeOffset(2026, 10, 1, 0, 0, 0, TimeSpan.Zero);
        var proposta = CriarEmitida(validaAte);
        proposta.Aceitar(validaAte.AddDays(-1));

        Assert.Throws<TransicaoInvalidaException>(() => proposta.Aceitar(validaAte.AddDays(-1)));
    }

    [Fact]
    public void Dada_PropostaJaPerdida_Quando_Aceita_Entao_Recusa()
    {
        var validaAte = new DateTimeOffset(2026, 10, 1, 0, 0, 0, TimeSpan.Zero);
        var proposta = CriarEmitida(validaAte);
        proposta.MarcarPerdida(validaAte.AddDays(-1), null);

        Assert.Throws<TransicaoInvalidaException>(() => proposta.Aceitar(validaAte.AddDays(-1)));
    }

    [Fact]
    public void Dada_PropostaEmitida_Quando_MarcaPerdidaComMotivo_Entao_PreencheMotivoEData()
    {
        var validaAte = new DateTimeOffset(2026, 10, 1, 0, 0, 0, TimeSpan.Zero);
        var proposta = CriarEmitida(validaAte);
        var agora = validaAte.AddDays(-2);

        proposta.MarcarPerdida(agora, "Cliente optou por outro fornecedor.");

        Assert.Equal(StatusProposta.Perdida, proposta.Status);
        Assert.Equal(agora, proposta.PerdidaEm);
        Assert.Equal("Cliente optou por outro fornecedor.", proposta.MotivoPerda);
    }

    [Fact]
    public void Dada_PropostaJaAceita_Quando_MarcaPerdida_Entao_Recusa()
    {
        var validaAte = new DateTimeOffset(2026, 10, 1, 0, 0, 0, TimeSpan.Zero);
        var proposta = CriarEmitida(validaAte);
        proposta.Aceitar(validaAte.AddDays(-1));

        Assert.Throws<TransicaoInvalidaException>(() => proposta.MarcarPerdida(validaAte.AddDays(-1), "motivo"));
    }

    [Fact]
    public void Dada_PropostaVencidaPeloRelogio_Quando_MarcaPerdida_Entao_Permite()
    {
        var validaAte = new DateTimeOffset(2026, 10, 1, 0, 0, 0, TimeSpan.Zero);
        var proposta = CriarEmitida(validaAte);

        proposta.MarcarPerdida(validaAte.AddDays(5), "Nao respondeu apos o vencimento.");

        Assert.Equal(StatusProposta.Perdida, proposta.Status);
    }

    [Fact]
    public void Dada_PropostaEmitidaDepoisDaValidade_Quando_Vence_Entao_MudaParaVencidaEPreencheVencidaEm()
    {
        var validaAte = new DateTimeOffset(2026, 10, 1, 0, 0, 0, TimeSpan.Zero);
        var proposta = CriarEmitida(validaAte);
        var agora = validaAte.AddSeconds(1);

        proposta.Vencer(agora);

        Assert.Equal(StatusProposta.Vencida, proposta.Status);
        Assert.Equal(agora, proposta.VencidaEm);
    }

    [Fact]
    public void Dada_PropostaEmitidaNoInstanteExatoDaValidade_Quando_Vence_Entao_Recusa()
    {
        var validaAte = new DateTimeOffset(2026, 10, 1, 0, 0, 0, TimeSpan.Zero);
        var proposta = CriarEmitida(validaAte);

        Assert.Throws<TransicaoInvalidaException>(() => proposta.Vencer(validaAte));
    }

    [Fact]
    public void Dada_PropostaForaDoStatusEmitida_Quando_Vence_Entao_Recusa()
    {
        var validaAte = new DateTimeOffset(2026, 10, 1, 0, 0, 0, TimeSpan.Zero);
        var proposta = CriarEmitida(validaAte);
        proposta.MarcarPerdida(validaAte.AddDays(-1), null);

        Assert.Throws<TransicaoInvalidaException>(() => proposta.Vencer(validaAte.AddSeconds(1)));
    }

    [Fact]
    public void Dada_PropostaVencida_Quando_MarcaVencimentoNotificado_Entao_PreencheData()
    {
        var validaAte = new DateTimeOffset(2026, 10, 1, 0, 0, 0, TimeSpan.Zero);
        var proposta = CriarEmitida(validaAte);
        var agora = validaAte.AddSeconds(1);
        proposta.Vencer(agora);

        proposta.MarcarVencimentoNotificado(agora);

        Assert.Equal(agora, proposta.VencimentoNotificadoEm);
    }

    [Fact]
    public void Dada_PropostaComVencimentoJaNotificado_Quando_MarcaNovamente_Entao_Recusa()
    {
        var validaAte = new DateTimeOffset(2026, 10, 1, 0, 0, 0, TimeSpan.Zero);
        var proposta = CriarEmitida(validaAte);
        var agora = validaAte.AddSeconds(1);
        proposta.Vencer(agora);
        proposta.MarcarVencimentoNotificado(agora);

        Assert.Throws<TransicaoInvalidaException>(() => proposta.MarcarVencimentoNotificado(agora));
    }
}
