using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SolarES.Api.Contratos;
using SolarES.Aplicacao.Propostas;
using SolarES.Dominio.Configuracao;
using SolarES.Dominio.Premissas;
using SolarES.Dominio.Proposta;
using SolarES.Infraestrutura.Persistencia;
using PropostaEntidade = SolarES.Dominio.Proposta.Proposta;

namespace SolarES.Api.Tests;

public sealed class EnviosPropostaApiTests : IClassFixture<SolarESApiFactory>
{
    private readonly SolarESApiFactory _factory;
    private readonly HttpClient _cliente;

    public EnviosPropostaApiTests(SolarESApiFactory factory)
    {
        _factory = factory;
        _cliente = factory.CreateClient();
    }

    [Fact]
    public async Task Dado_Anonimo_Quando_SolicitaListaOuReenviaEnvio_Entao_RetornaNaoAutorizado()
    {
        var id = await PrepararPropostaAsync(calibracaoPendente: false, comPdf: false);

        Assert.Equal(HttpStatusCode.Unauthorized,
            (await _cliente.PostAsJsonAsync($"/api/propostas/{id}/envios", new SolicitarEnvioPropostaRequest(CanalEnvio.Email, "x@x.com"))).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await _cliente.GetAsync($"/api/propostas/{id}/envios")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized,
            (await _cliente.PostAsync($"/api/propostas/{id}/envios/{Guid.NewGuid()}/reenvio", null)).StatusCode);
    }

    [Fact]
    public async Task Dada_PropostaSobCalibracaoPendente_Quando_SolicitaEnvio_Entao_RetornaConflitoSemPersistir()
    {
        var id = await PrepararPropostaAsync(calibracaoPendente: true, comPdf: false);
        await SolarESApiFactory.ClienteAutenticadoAsync(_cliente, SolarESApiFactory.VendedorEmail, SolarESApiFactory.VendedorSenha);

        var resposta = await _cliente.PostAsJsonAsync($"/api/propostas/{id}/envios",
            new SolicitarEnvioPropostaRequest(CanalEnvio.Email, "cliente@exemplo.com"));

        Assert.Equal(HttpStatusCode.Conflict, resposta.StatusCode);
        using var escopo = _factory.Services.CreateScope();
        var banco = escopo.ServiceProvider.GetRequiredService<SolarESDbContext>();
        Assert.Empty(await banco.EnviosProposta.Where(e => e.PropostaId == id).ToListAsync());
    }

    [Fact]
    public async Task Dada_PropostaConfirmadaComPdf_Quando_SolicitaEnvio_Entao_MarcaEnviadoEApareceNoHistorico()
    {
        var id = await PrepararPropostaAsync(calibracaoPendente: false, comPdf: true);
        await SolarESApiFactory.ClienteAutenticadoAsync(_cliente, SolarESApiFactory.VendedorEmail, SolarESApiFactory.VendedorSenha);

        var resposta = await _cliente.PostAsJsonAsync($"/api/propostas/{id}/envios",
            new SolicitarEnvioPropostaRequest(CanalEnvio.Email, "cliente@exemplo.com"));
        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
        var envioCriado = await resposta.Content.ReadFromJsonAsync<EnvioPropostaResponse>();

        EnvioPropostaResponse? envioFinal = null;
        for (var tentativa = 0; tentativa < 25 && envioFinal?.Status != StatusEnvioProposta.Enviado; tentativa++)
        {
            await Task.Delay(200);
            var lista = await _cliente.GetFromJsonAsync<List<EnvioPropostaResponse>>($"/api/propostas/{id}/envios");
            envioFinal = lista!.Single(e => e.Id == envioCriado!.Id);
        }

        Assert.Equal(StatusEnvioProposta.Enviado, envioFinal!.Status);
        var detalheProposta = await _cliente.GetFromJsonAsync<PropostaResponse>($"/api/propostas/{id}");
        Assert.NotNull(detalheProposta!.EnviadaEm);
        Assert.Equal(CanalEnvio.Email, detalheProposta.Canal);
    }

    [Fact]
    public async Task Dado_EnvioInexistente_Quando_Reenvia_Entao_RetornaNaoEncontrado()
    {
        var id = await PrepararPropostaAsync(calibracaoPendente: false, comPdf: false);
        await SolarESApiFactory.ClienteAutenticadoAsync(_cliente, SolarESApiFactory.VendedorEmail, SolarESApiFactory.VendedorSenha);

        var resposta = await _cliente.PostAsync($"/api/propostas/{id}/envios/{Guid.NewGuid()}/reenvio", null);

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
    }

    private async Task<Guid> PrepararPropostaAsync(bool calibracaoPendente, bool comPdf)
    {
        using var escopo = _factory.Services.CreateScope();
        var banco = escopo.ServiceProvider.GetRequiredService<SolarESDbContext>();
        var payload = calibracaoPendente ? ConfiguracaoCalculoBaseline.Criar() : ConfiguracaoConfirmada();
        var versao = ConfiguracaoVersao.CriarRascunho((await banco.ConfiguracoesVersao.CountAsync()) + 1, payload);
        versao.Publicar(Guid.NewGuid(), DateTimeOffset.UtcNow);
        banco.Add(versao);

        var agora = DateTimeOffset.UtcNow;
        var proposta = new PropostaEntidade
        {
            Id = Guid.NewGuid(),
            SimulacaoId = Guid.NewGuid(),
            Numero = $"PROP-TESTE-{Guid.NewGuid():N}",
            ConfiguracaoVersaoId = versao.Id,
            ValidaAte = agora.AddDays(15),
            Status = StatusProposta.Emitida,
            CriadoEm = agora,
            AtualizadoEm = agora,
        };
        if (comPdf)
        {
            var armazenamentoPdf = escopo.ServiceProvider.GetRequiredService<IArmazenamentoPdf>();
            proposta.ArquivoPdfUrl = await armazenamentoPdf.SalvarAsync(
                proposta.Numero, "%PDF-1.4 conteudo"u8.ToArray(), CancellationToken.None);
        }
        banco.Propostas.Add(proposta);
        await banco.SaveChangesAsync();
        return proposta.Id;
    }

    private static ConfiguracaoCalculo ConfiguracaoConfirmada()
    {
        var b = ConfiguracaoCalculoBaseline.Criar();
        return new(
            new(b.PerformanceRatio.Valor, OrigemPremissa.FontePublica),
            new(b.DegradacaoAnual.Valor, OrigemPremissa.FontePublica),
            new(b.InflacaoTarifaria.Valor, OrigemPremissa.FontePublica),
            new(b.TaxaDesconto.Valor, OrigemPremissa.FontePublica),
            new(b.HorizonteAnos.Valor, OrigemPremissa.FontePublica),
            new(b.OversizingMaximo.Valor, OrigemPremissa.FontePublica),
            new(b.FatorOrientacaoPadrao.Valor, OrigemPremissa.FontePublica),
            b.CronogramaFioB,
            new(b.EstrategiaFioBForaCronograma.Valor, OrigemPremissa.FontePublica),
            new(b.LimiteKwpRoteamentoHumano.Valor, OrigemPremissa.FontePublica),
            new(b.KitLitoral.Valor, OrigemPremissa.FontePublica),
            b.CustoDisponibilidadePorLigacao,
            new(b.TextosProposta.Valor, OrigemPremissa.FontePublica));
    }
}
