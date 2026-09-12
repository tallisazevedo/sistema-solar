using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SolarES.Api.Contratos;
using SolarES.Aplicacao.Leads;
using SolarES.Dominio.Catalogo;
using SolarES.Dominio.Configuracao;
using SolarES.Dominio.Lead;
using SolarES.Dominio.Premissas;
using SolarES.Dominio.Simulacao;
using SolarES.Dominio.Tarifas;
using SolarES.Infraestrutura.Persistencia;

namespace SolarES.Api.Tests;

/// <summary>Issue #33: limite de envios/leads por destino em 24h no fluxo publico.</summary>
public sealed class LimiteEnviosApiTests
{
    [Fact]
    public async Task QuartoLead_ComMesmoContato_RegistraLeadSemEnvioAutomatico_ComRespostaIdenticaACalibracaoPendente()
    {
        await using var factory = new SolarESApiFactoryLimiteEnvios();
        var cliente = factory.CreateClient();
        const string codigoIbge = "5555555";
        await PrepararCenarioSemProvisorioAsync(factory, cliente, codigoIbge);
        cliente.DefaultRequestHeaders.Authorization = null;

        var primeiraSimulacao = await CriarSimulacaoAsync(cliente, codigoIbge);
        using (var primeiroFormulario = FormularioLead())
        {
            var primeiraResposta = await cliente.PostAsync(
                $"/api/publico/simulacoes/{primeiraSimulacao}/lead", primeiroFormulario);
            var primeiroResultado = await primeiraResposta.Content.ReadFromJsonAsync<CapturarLeadPublicoResponse>();
            Assert.Equal(nameof(DesfechoCapturaLead.PropostaEmitida), primeiroResultado!.Desfecho);
        }

        var segundaSimulacao = await CriarSimulacaoAsync(cliente, codigoIbge);
        using var segundoFormulario = FormularioLead();
        var segundaResposta = await cliente.PostAsync(
            $"/api/publico/simulacoes/{segundaSimulacao}/lead", segundoFormulario);
        var segundoResultado = await segundaResposta.Content.ReadFromJsonAsync<CapturarLeadPublicoResponse>();

        // Mesma resposta de calibracao pendente -- o visitante nao percebe que existe um limite.
        Assert.Equal(nameof(DesfechoCapturaLead.CalibracaoPendente), segundoResultado!.Desfecho);

        using var escopo = factory.Services.CreateScope();
        var banco = escopo.ServiceProvider.GetRequiredService<SolarESDbContext>();
        Assert.Equal(2, await banco.Leads.CountAsync(l => l.Email == "maria@exemplo.com"));
        Assert.False(await banco.Propostas.AnyAsync(p => p.SimulacaoId == segundaSimulacao));
    }

    private static async Task<Guid> CriarSimulacaoAsync(HttpClient cliente, string codigoIbge)
    {
        var criar = await cliente.PostAsJsonAsync("/api/publico/simulacoes", new CriarSimulacaoPublicaRequest(
            500m, null, TipoLigacao.Monofasica, PerfilImovel.Residencial, codigoIbge,
            TipoTelhado.Ceramico, AreaDisponivelM2: 1000m, PossuiGeracaoPropria: false));
        var resultado = await criar.Content.ReadFromJsonAsync<SimulacaoPublicaResponse>();
        return resultado!.Id;
    }

    private static MultipartFormDataContent FormularioLead()
    {
        return new MultipartFormDataContent
        {
            { new StringContent("Maria Silva"), "Nome" },
            { new StringContent("27999999999"), "Telefone" },
            { new StringContent("maria@exemplo.com"), "Email" },
            { new StringContent(nameof(CanalPreferido.Email)), "CanalPreferido" },
            { new StringContent(nameof(FinalidadeConsentimento.ContatoComercial)), "FinalidadesAceitas" },
            { new StringContent("contato-comercial-v1"), "VersaoTexto" },
        };
    }

    private static async Task PrepararCenarioSemProvisorioAsync(SolarESApiFactoryLimiteEnvios factory, HttpClient cliente, string codigoIbge)
    {
        await SolarESApiFactory.ClienteAutenticadoAsync(cliente, SolarESApiFactory.DonoEmail, SolarESApiFactory.DonoSenha);
        var sufixo = Guid.NewGuid().ToString("N")[..8];
        await cliente.PostAsJsonAsync("/api/catalogo/modulos-fotovoltaicos", new ModuloFotovoltaicoRequest(
            "Fabricante", $"Modulo-{sufixo}", 550, 1134, 2278, 21.3m, false, true));
        await cliente.PostAsJsonAsync("/api/catalogo/inversores", new InversorRequest(
            "Fabricante", $"Inversor-{sufixo}", 10000, 2, TipoInversor.String, true));
        var distribuidoraHttp = await cliente.PostAsJsonAsync("/api/tarifas/distribuidoras",
            new DistribuidoraRequest("Distribuidora Publica", $"DP-{sufixo}", true));
        var distribuidora = await distribuidoraHttp.Content.ReadFromJsonAsync<DistribuidoraResponse>();
        await cliente.PostAsJsonAsync("/api/tarifas/municipios", new MunicipioHspRequest(
            codigoIbge, "Municipio Publico", -20m, -40m, 200m, distribuidora!.Id,
            Enumerable.Repeat(5m, 12).ToList(), "Teste"));
        await cliente.PostAsJsonAsync("/api/tarifas/vigentes", new TarifaVigenteRequest(
            distribuidora.Id, Subgrupo.B1, .35m, .40m, .21m, .17m, .0925m,
            DateTimeOffset.UtcNow.AddDays(-1), null, "Teste", "Teste"));
        await cliente.PostAsJsonAsync("/api/precificacao/faixas-preco", new FaixaPrecoRequest(
            0m, 500m, 3.5m, "Residencial", false, DateTimeOffset.UtcNow.AddDays(-1)));

        using var escopo = factory.Services.CreateScope();
        var contexto = escopo.ServiceProvider.GetRequiredService<SolarESDbContext>();
        var b = ConfiguracaoCalculoBaseline.Criar();
        var semProvisorio = new ConfiguracaoCalculo(
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

        var publicada = await contexto.ConfiguracoesVersao.SingleOrDefaultAsync(c => c.Status == StatusConfiguracaoVersao.Publicada);
        publicada?.Arquivar();
        var versao = ConfiguracaoVersao.CriarRascunho((publicada?.Numero ?? 0) + 1, semProvisorio, "Teste");
        versao.Publicar(Guid.NewGuid(), DateTimeOffset.UtcNow);
        contexto.ConfiguracoesVersao.Add(versao);
        await contexto.SaveChangesAsync();
    }
}
