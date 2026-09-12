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
        Assert.Equal(4.4m, detalhe.Resultado!.PotenciaInstaladaKwp);
    }

    [Fact]
    public async Task Dado_Vendedor_Quando_CadastraLeadManualEAlteraStatus_Entao_PersisteHistorico()
    {
        await SolarESApiFactory.ClienteAutenticadoAsync(_cliente, SolarESApiFactory.VendedorEmail,
            SolarESApiFactory.VendedorSenha);
        var respostaCriacao = await _cliente.PostAsJsonAsync("/api/leads", new CriarLeadManualRequest(
            "Cliente indicado", "27999999999", "indicado@teste.com", OrigemLead.Indicacao, true,
            "contato-comercial-v1"));
        Assert.Equal(HttpStatusCode.Created, respostaCriacao.StatusCode);
        var lead = await respostaCriacao.Content.ReadFromJsonAsync<LeadResponse>();

        var respostaStatus = await _cliente.PatchAsJsonAsync($"/api/leads/{lead!.Id}/status",
            new AlterarStatusLeadRequest(StatusLead.EmAtendimento, null));

        Assert.Equal(HttpStatusCode.NoContent, respostaStatus.StatusCode);
        using var escopo = _factory.Services.CreateScope();
        var banco = escopo.ServiceProvider.GetRequiredService<SolarESDbContext>();
        Assert.Single(await banco.HistoricosStatusLead.Where(h => h.LeadId == lead.Id).ToListAsync());
        Assert.Equal(StatusLead.EmAtendimento, (await banco.Leads.SingleAsync(l => l.Id == lead.Id)).Status);
    }

    [Fact]
    public async Task Dado_CadastroManualSemConsentimentoOuComOrigemLanding_Quando_Cria_Entao_Recusa()
    {
        await SolarESApiFactory.ClienteAutenticadoAsync(_cliente, SolarESApiFactory.VendedorEmail,
            SolarESApiFactory.VendedorSenha);
        var semConsentimento = await _cliente.PostAsJsonAsync("/api/leads", new CriarLeadManualRequest(
            "Cliente", "27999999999", "cliente@teste.com", OrigemLead.Indicacao, false, "v1"));
        var landing = await _cliente.PostAsJsonAsync("/api/leads", new CriarLeadManualRequest(
            "Cliente", "27999999999", "cliente2@teste.com", OrigemLead.Landing, true, "v1"));

        Assert.Equal(HttpStatusCode.BadRequest, semConsentimento.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, landing.StatusCode);
    }

    [Fact]
    public async Task Dado_FiltrosDeOrigemEStatus_Quando_Lista_Entao_RetornaSomenteCorrespondentes()
    {
        await SolarESApiFactory.ClienteAutenticadoAsync(_cliente, SolarESApiFactory.VendedorEmail,
            SolarESApiFactory.VendedorSenha);
        await _cliente.PostAsJsonAsync("/api/leads", new CriarLeadManualRequest(
            "Lead telefone", "27999999991", "telefone@teste.com", OrigemLead.Telefone, true,
            "contato-comercial-v1"));
        await _cliente.PostAsJsonAsync("/api/leads", new CriarLeadManualRequest(
            "Lead indicação", "27999999992", "indicacao@teste.com", OrigemLead.Indicacao, true,
            "contato-comercial-v1"));

        var encontrados = await _cliente.GetFromJsonAsync<List<LeadResponse>>(
            $"/api/leads?origem={(int)OrigemLead.Telefone}&status={(int)StatusLead.Novo}");

        Assert.NotEmpty(encontrados!);
        Assert.All(encontrados!, lead => Assert.Equal(OrigemLead.Telefone, lead.Origem));
        Assert.All(encontrados!, lead => Assert.Equal(StatusLead.Novo, lead.Status));
    }

    private async Task<Guid> PrepararLeadAsync()
    {
        using var escopo = _factory.Services.CreateScope();
        var banco = escopo.ServiceProvider.GetRequiredService<SolarESDbContext>();
        var versao = await banco.ConfiguracoesVersao.FirstOrDefaultAsync(c => c.Status == StatusConfiguracaoVersao.Publicada);
        if (versao is null) { versao = ConfiguracaoVersao.CriarRascunho(1, ConfiguracaoCalculoBaseline.Criar()); versao.Publicar(Guid.NewGuid(), DateTimeOffset.UtcNow); banco.Add(versao); }
        var resultado = new ResultadoSimulacao(4.4m, 8, 20m, 100m, 15000m, 400m, 40, 50, .2m, 10000m, true, "Engenharia", false, true, []);
        var simulacao = new Simulacao { Id = Guid.NewGuid(), ConfiguracaoVersaoId = versao.Id, Origem = OrigemSimulacao.Landing, EntradasSnapshot = "{}", ResultadoSnapshot = JsonSerializer.Serialize(resultado), PotenciaKwp = 4.4m, QuantidadeModulos = 8, Capex = 15000m, EconomiaMensalAno1 = 400m, CoberturaPercentual = 100m, RoteadaParaHumano = true, CriadoEm = DateTimeOffset.UtcNow, AtualizadoEm = DateTimeOffset.UtcNow };
        var lead = LeadEntidade.Criar("Lead recente", "27999999999", "lead@teste.com", CanalPreferido.Whatsapp, simulacao.Id, Guid.NewGuid(), [FinalidadeConsentimento.ContatoComercial], DateTimeOffset.UtcNow.AddMinutes(1));
        simulacao.LeadId = lead.Id;
        banco.AddRange(simulacao, lead);
        await banco.SaveChangesAsync();
        return lead.Id;
    }
}
