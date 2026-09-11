using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SolarES.Api.Contratos;
using SolarES.Dominio.Configuracao;
using SolarES.Dominio.Lead;
using SolarES.Dominio.Simulacao;
using SolarES.Infraestrutura.Persistencia;
using LeadEntidade = SolarES.Dominio.Lead.Lead;

namespace SolarES.Api.Tests;

public sealed class LeadsApiTests : IClassFixture<SolarESApiFactory>
{
    private readonly SolarESApiFactory _factory;
    private readonly HttpClient _cliente;
    public LeadsApiTests(SolarESApiFactory factory) { _factory = factory; _cliente = factory.CreateClient(); }

    [Fact]
    public async Task Dado_Anonimo_Quando_ConsultaLeads_Entao_RetornaNaoAutorizado()
    {
        Assert.Equal(HttpStatusCode.Unauthorized, (await _cliente.GetAsync("/api/leads")).StatusCode);
    }

    [Fact]
    public async Task Dado_Vendedor_Quando_ConsultaLeads_Entao_RetornaMaisRecenteComDetalhe()
    {
        var id = await PrepararLeadAsync();
        await SolarESApiFactory.ClienteAutenticadoAsync(_cliente, SolarESApiFactory.VendedorEmail, SolarESApiFactory.VendedorSenha);

        var lista = await _cliente.GetFromJsonAsync<List<LeadResponse>>("/api/leads");
        var detalhe = await _cliente.GetFromJsonAsync<LeadResponse>($"/api/leads/{id}");

        var primeira = (lista ?? throw new InvalidOperationException("Lista ausente.")).First();
        Assert.Equal(id, primeira.Id);
        Assert.True(primeira.RoteadoParaHumano);
        Assert.True(primeira.CalibracaoPendente);
        Assert.Equal("Lead recente", detalhe!.Nome);
        Assert.Equal(CanalPreferido.Whatsapp, detalhe.CanalPreferido);
        Assert.Equal(4.4m, detalhe.Resultado.PotenciaInstaladaKwp);
    }

    private async Task<Guid> PrepararLeadAsync()
    {
        using var escopo = _factory.Services.CreateScope();
        var banco = escopo.ServiceProvider.GetRequiredService<SolarESDbContext>();
        var versao = await banco.ConfiguracoesVersao.FirstOrDefaultAsync(c => c.Status == StatusConfiguracaoVersao.Publicada);
        if (versao is null) { versao = ConfiguracaoVersao.CriarRascunho(1, ConfiguracaoCalculoBaseline.Criar()); versao.Publicar(Guid.NewGuid(), DateTimeOffset.UtcNow); banco.Add(versao); }
        var resultado = new ResultadoSimulacao(4.4m, 8, 20m, 100m, 15000m, 400m, 40, 50, .2m, 10000m, true, "Engenharia", false, true, []);
        var simulacao = new Simulacao { Id = Guid.NewGuid(), ConfiguracaoVersaoId = versao.Id, Origem = OrigemSimulacao.Landing, EntradasSnapshot = "{}", ResultadoSnapshot = JsonSerializer.Serialize(resultado), PotenciaKwp = 4.4m, QuantidadeModulos = 8, Capex = 15000m, EconomiaMensalAno1 = 400m, CoberturaPercentual = 100m, RoteadaParaHumano = true, CriadoEm = DateTimeOffset.UtcNow, AtualizadoEm = DateTimeOffset.UtcNow };
        var lead = LeadEntidade.Criar("Lead recente", "27999999999", "lead@teste.com", CanalPreferido.Whatsapp, simulacao.Id, Guid.NewGuid(), true, DateTimeOffset.UtcNow.AddMinutes(1));
        simulacao.LeadId = lead.Id;
        banco.AddRange(simulacao, lead);
        await banco.SaveChangesAsync();
        return lead.Id;
    }
}
