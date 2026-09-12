using System.Net;
using System.Net.Http.Json;
using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using SolarES.Api.Contratos;
using SolarES.Dominio.Metricas;
using SolarES.Infraestrutura.Persistencia;

namespace SolarES.Api.Tests;

public sealed class MetricasFunilApiTests : IClassFixture<SolarESApiFactory>
{
    private readonly SolarESApiFactory _factory;
    private readonly HttpClient _cliente;
    public MetricasFunilApiTests(SolarESApiFactory factory) { _factory = factory; _cliente = factory.CreateClient(); }

    [Fact]
    public async Task Dadas_DezSessoesIniciadasEQuatroConcluidas_Quando_ConsultaPeriodo_Entao_RetornaQuarentaPorCento()
    {
        var agora = DateTimeOffset.UtcNow;
        using (var escopo = _factory.Services.CreateScope())
        {
            var banco = escopo.ServiceProvider.GetRequiredService<SolarESDbContext>();
            for (var indice = 0; indice < 10; indice++)
            {
                var sessao = Guid.NewGuid();
                banco.EventosFunil.Add(EventoFunil.CriarInicio(sessao, agora));
                if (indice < 3)
                    banco.EventosFunil.Add(EventoFunil.CriarConclusao(sessao, Guid.NewGuid(), agora));
            }
            var sessaoIniciadaAntesDoPeriodo = Guid.NewGuid();
            banco.EventosFunil.Add(EventoFunil.CriarInicio(sessaoIniciadaAntesDoPeriodo, agora.AddDays(-10)));
            banco.EventosFunil.Add(EventoFunil.CriarConclusao(sessaoIniciadaAntesDoPeriodo, Guid.NewGuid(), agora));
            await banco.SaveChangesAsync();
        }
        await SolarESApiFactory.ClienteAutenticadoAsync(_cliente, SolarESApiFactory.DonoEmail,
            SolarESApiFactory.DonoSenha);

        var resposta = await _cliente.GetFromJsonAsync<MetricasFunilResponse>(
            $"/api/metricas/funil?de={Uri.EscapeDataString(agora.AddHours(-1).ToString("O", CultureInfo.InvariantCulture))}&ate={Uri.EscapeDataString(agora.AddHours(1).ToString("O", CultureInfo.InvariantCulture))}");

        Assert.Equal(10, resposta!.SessoesIniciadas);
        Assert.Equal(4, resposta.SessoesConcluidas);
        Assert.Equal(40m, resposta.TaxaConclusaoPercentual);
    }

    [Fact]
    public async Task Dado_Vendedor_Quando_ConsultaFunil_Entao_RetornaProibido()
    {
        await SolarESApiFactory.ClienteAutenticadoAsync(_cliente, SolarESApiFactory.VendedorEmail,
            SolarESApiFactory.VendedorSenha);
        var resposta = await _cliente.GetAsync(
            $"/api/metricas/funil?de={Uri.EscapeDataString(DateTimeOffset.UtcNow.AddDays(-1).ToString("O", CultureInfo.InvariantCulture))}&ate={Uri.EscapeDataString(DateTimeOffset.UtcNow.ToString("O", CultureInfo.InvariantCulture))}");
        Assert.Equal(HttpStatusCode.Forbidden, resposta.StatusCode);
    }

    [Fact]
    public async Task Dado_EventoPublico_Quando_RegistraInicio_Entao_AceitaMasRecusaConclusao()
    {
        var inicio = await _cliente.PostAsJsonAsync("/api/publico/eventos",
            new RegistrarEventoFunilRequest(TipoEventoFunil.SimulacaoIniciada, Guid.NewGuid()));
        var conclusao = await _cliente.PostAsJsonAsync("/api/publico/eventos",
            new RegistrarEventoFunilRequest(TipoEventoFunil.SimulacaoConcluida, Guid.NewGuid()));
        Assert.Equal(HttpStatusCode.Accepted, inicio.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, conclusao.StatusCode);
    }
}
