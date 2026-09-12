using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SolarES.Api.Contratos;
using SolarES.Dominio.Catalogo;
using SolarES.Dominio.Configuracao;
using SolarES.Dominio.Simulacao;
using SolarES.Dominio.Tarifas;
using SolarES.Dominio.Lead;
using SolarES.Aplicacao.Leads;
using SolarES.Dominio.Premissas;
using SolarES.Dominio.Proposta;
using SolarES.Infraestrutura.Persistencia;

namespace SolarES.Api.Tests;

public sealed class SimulacoesPublicasApiTests : IClassFixture<SolarESApiFactory>
{
    private readonly SolarESApiFactory _factory;
    private readonly HttpClient _cliente;

    public SimulacoesPublicasApiTests(SolarESApiFactory factory)
    {
        _factory = factory;
        _cliente = factory.CreateClient();
    }

    [Fact]
    public async Task Dado_VisitanteSemAutenticacao_Quando_CriaEReabreSimulacao_Entao_RetornaMesmoResultado()
    {
        const string codigoIbge = "8888888";
        await PrepararCenarioAsync(codigoIbge);
        _cliente.DefaultRequestHeaders.Authorization = null;

        var municipios = await _cliente.GetFromJsonAsync<List<MunicipioPublicoResponse>>("/api/publico/municipios");
        Assert.Contains(municipios!, municipio => municipio.CodigoIbge == codigoIbge && municipio.Nome == "Municipio Publico");

        var request = new CriarSimulacaoPublicaRequest(
            500m, null, TipoLigacao.Monofasica, PerfilImovel.Residencial, codigoIbge,
            TipoTelhado.Ceramico, 1000m, PossuiGeracaoPropria: false);
        var criar = await _cliente.PostAsJsonAsync("/api/publico/simulacoes", request);

        Assert.Equal(HttpStatusCode.Created, criar.StatusCode);
        var resultado = await criar.Content.ReadFromJsonAsync<SimulacaoPublicaResponse>();
        Assert.NotNull(resultado);
        Assert.True(resultado!.PotenciaKwp > 0);
        Assert.True(resultado.CalibracaoPendente);

        var reaberta = await _cliente.GetFromJsonAsync<SimulacaoPublicaResponse>($"/api/publico/simulacoes/{resultado.Id}");
        Assert.Equal(resultado.Id, reaberta!.Id);
        Assert.Equal(resultado.PotenciaKwp, reaberta.PotenciaKwp);
        Assert.Equal(resultado.Projecao, reaberta.Projecao);

        using var escopo = _factory.Services.CreateScope();
        var contexto = escopo.ServiceProvider.GetRequiredService<SolarESDbContext>();
        var persistida = await contexto.Simulacoes.SingleAsync(s => s.Id == resultado.Id);
        var versaoPublicada = await contexto.ConfiguracoesVersao.SingleAsync(
            c => c.Status == StatusConfiguracaoVersao.Publicada);
        Assert.Equal(OrigemSimulacao.Landing, persistida.Origem);
        Assert.Equal(versaoPublicada.Id, persistida.ConfiguracaoVersaoId);
        using var entradas = JsonDocument.Parse(persistida.EntradasSnapshot);
        var historico = entradas.RootElement.GetProperty("HistoricoConsumoKwh");
        Assert.Equal(12, historico.GetArrayLength());
        Assert.All(historico.EnumerateArray(), consumo => Assert.Equal(500m, consumo.GetDecimal()));
        Assert.False(string.IsNullOrWhiteSpace(persistida.ResultadoSnapshot));
    }

    [Fact]
    public async Task Dada_SimulacaoInterna_Quando_ConsultadaNaSuperficiePublica_Entao_RetornaNaoEncontrada()
    {
        const string codigoIbge = "7777777";
        await PrepararCenarioAsync(codigoIbge);
        var interna = await _cliente.PostAsJsonAsync("/api/simulacoes", new EntradaSimulacaoRequest(
            Enumerable.Repeat(500m, 12).ToList(), TipoLigacao.Monofasica, Subgrupo.B1,
            codigoIbge, TipoTelhado.Ceramico, 1000m, false));
        var detalhe = await interna.Content.ReadFromJsonAsync<SimulacaoDetalheResponse>();
        _cliente.DefaultRequestHeaders.Authorization = null;

        var response = await _cliente.GetAsync($"/api/publico/simulacoes/{detalhe!.Id}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Dado_HistoricoMensal_Quando_CriaSimulacao_Entao_PreservaOsDozeValores()
    {
        const string codigoIbge = "6666666";
        await PrepararCenarioAsync(codigoIbge);
        _cliente.DefaultRequestHeaders.Authorization = null;
        var historico = Enumerable.Range(1, 12).Select(mes => 300m + mes).ToList();
        var request = new CriarSimulacaoPublicaRequest(null, historico, TipoLigacao.Monofasica,
            PerfilImovel.Residencial, codigoIbge, TipoTelhado.Ceramico, 1000m, false);

        var response = await _cliente.PostAsJsonAsync("/api/publico/simulacoes", request);
        var resultado = await response.Content.ReadFromJsonAsync<SimulacaoPublicaResponse>();

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        using var escopo = _factory.Services.CreateScope();
        var contexto = escopo.ServiceProvider.GetRequiredService<SolarESDbContext>();
        var persistida = await contexto.Simulacoes.SingleAsync(s => s.Id == resultado!.Id);
        using var entradas = JsonDocument.Parse(persistida.EntradasSnapshot);
        Assert.Equal(historico, entradas.RootElement.GetProperty("HistoricoConsumoKwh")
            .EnumerateArray().Select(valor => valor.GetDecimal()).ToList());
    }

    [Fact]
    public async Task Dado_CenarioRoteado_Quando_CriaSimulacao_Entao_NaoExpoeNumeros()
    {
        const string codigoIbge = "5555555";
        await PrepararCenarioAsync(codigoIbge);
        _cliente.DefaultRequestHeaders.Authorization = null;
        var request = new CriarSimulacaoPublicaRequest(500m, null, TipoLigacao.Monofasica,
            PerfilImovel.Residencial, codigoIbge, TipoTelhado.Ceramico, 1000m, true);

        var response = await _cliente.PostAsJsonAsync("/api/publico/simulacoes", request);
        var resultado = await response.Content.ReadFromJsonAsync<SimulacaoPublicaResponse>();

        Assert.True(resultado!.RoteadaParaHumano);
        Assert.Null(resultado.PotenciaKwp);
        Assert.Null(resultado.QuantidadeModulos);
        Assert.Null(resultado.InvestimentoEstimado);
        Assert.Null(resultado.EconomiaMensalAno1);
        Assert.Null(resultado.Projecao);
    }

    [Fact]
    public async Task Dadas_OrigensDiferentes_Quando_PreflightPublico_Entao_LiberaSomenteAConfigurada()
    {
        using var permitida = new HttpRequestMessage(HttpMethod.Options, "/api/publico/municipios");
        permitida.Headers.Add("Origin", "https://landing.solares.test");
        permitida.Headers.Add("Access-Control-Request-Method", "GET");
        var respostaPermitida = await _cliente.SendAsync(permitida);

        Assert.Equal("https://landing.solares.test", respostaPermitida.Headers.GetValues("Access-Control-Allow-Origin").Single());

        using var negada = new HttpRequestMessage(HttpMethod.Options, "/api/publico/municipios");
        negada.Headers.Add("Origin", "https://nao-permitida.test");
        negada.Headers.Add("Access-Control-Request-Method", "GET");
        var respostaNegada = await _cliente.SendAsync(negada);

        Assert.False(respostaNegada.Headers.Contains("Access-Control-Allow-Origin"));

        using var administrativa = new HttpRequestMessage(HttpMethod.Options, "/api/simulacoes");
        administrativa.Headers.Add("Origin", "https://landing.solares.test");
        administrativa.Headers.Add("Access-Control-Request-Method", "GET");
        var respostaAdministrativa = await _cliente.SendAsync(administrativa);

        Assert.False(respostaAdministrativa.Headers.Contains("Access-Control-Allow-Origin"));
    }

    [Fact]
    public async Task Dada_CalibracaoPendente_Quando_CapturaLead_Entao_NaoEmiteProposta()
    {
        const string codigoIbge = "4444444";
        await PrepararCenarioAsync(codigoIbge);
        _cliente.DefaultRequestHeaders.Authorization = null;
        var criar = await _cliente.PostAsJsonAsync("/api/publico/simulacoes",
            new CriarSimulacaoPublicaRequest(500m, null, TipoLigacao.Monofasica,
                PerfilImovel.Residencial, codigoIbge, TipoTelhado.Ceramico, 1000m, false));
        var simulacao = await criar.Content.ReadFromJsonAsync<SimulacaoPublicaResponse>();

        using var formulario = FormularioLead(consentimento: true);
        var response = await _cliente.PostAsync($"/api/publico/simulacoes/{simulacao!.Id}/lead", formulario);
        var resultado = await response.Content.ReadFromJsonAsync<CapturarLeadPublicoResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(nameof(DesfechoCapturaLead.CalibracaoPendente), resultado!.Desfecho);
        using var escopo = _factory.Services.CreateScope();
        var contexto = escopo.ServiceProvider.GetRequiredService<SolarESDbContext>();
        Assert.Contains(await contexto.Leads.ToListAsync(), lead => lead.SimulacaoId == simulacao.Id);
        var leadCriado = await contexto.Leads.SingleAsync(lead => lead.SimulacaoId == simulacao.Id);
        var consentimento = await contexto.ConsentimentosLgpd.SingleAsync(c => c.LeadId == leadCriado.Id);
        Assert.Equal(FinalidadeConsentimento.ContatoComercial, consentimento.Finalidade);
        Assert.Equal("contato-comercial-v1", consentimento.VersaoTexto);
        Assert.Equal(leadCriado.ConsentimentoLgpdEm, consentimento.ConcedidoEm);
        // Nenhuma Proposta foi criada pra esta simulacao -- e como EnvioProposta so
        // existe a partir de uma Proposta ja' persistida, isso ja' prova que nenhum
        // envio foi enfileirado (o teste nao pode checar a tabela inteira: ela e'
        // compartilhada entre os testes desta classe).
        Assert.DoesNotContain(await contexto.Propostas.ToListAsync(), proposta => proposta.SimulacaoId == simulacao.Id);
    }

    [Fact]
    public async Task Dado_ConsentimentoAusente_Quando_CapturaLead_Entao_NaoPersisteLead()
    {
        const string codigoIbge = "3333333";
        await PrepararCenarioAsync(codigoIbge);
        _cliente.DefaultRequestHeaders.Authorization = null;
        var criar = await _cliente.PostAsJsonAsync("/api/publico/simulacoes",
            new CriarSimulacaoPublicaRequest(500m, null, TipoLigacao.Monofasica,
                PerfilImovel.Residencial, codigoIbge, TipoTelhado.Ceramico, 1000m, false));
        var simulacao = await criar.Content.ReadFromJsonAsync<SimulacaoPublicaResponse>();

        using var formulario = FormularioLead(consentimento: false);
        var response = await _cliente.PostAsync($"/api/publico/simulacoes/{simulacao!.Id}/lead", formulario);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        using var escopo = _factory.Services.CreateScope();
        var contexto = escopo.ServiceProvider.GetRequiredService<SolarESDbContext>();
        Assert.DoesNotContain(await contexto.Leads.ToListAsync(), lead => lead.SimulacaoId == simulacao.Id);
    }

    [Fact]
    public async Task Dada_VersaoDesconhecida_Quando_CapturaLead_Entao_RecusaSemPersistir()
    {
        const string codigoIbge = "1414141";
        await PrepararCenarioAsync(codigoIbge);
        _cliente.DefaultRequestHeaders.Authorization = null;
        var criar = await _cliente.PostAsJsonAsync("/api/publico/simulacoes",
            new CriarSimulacaoPublicaRequest(500m, null, TipoLigacao.Monofasica,
                PerfilImovel.Residencial, codigoIbge, TipoTelhado.Ceramico, 1000m, false));
        var simulacao = await criar.Content.ReadFromJsonAsync<SimulacaoPublicaResponse>();

        using var formulario = FormularioLead(true, versaoTexto: "versao-inventada");
        var response = await _cliente.PostAsync($"/api/publico/simulacoes/{simulacao!.Id}/lead", formulario);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        using var escopo = _factory.Services.CreateScope();
        var contexto = escopo.ServiceProvider.GetRequiredService<SolarESDbContext>();
        Assert.DoesNotContain(await contexto.Leads.ToListAsync(), lead => lead.SimulacaoId == simulacao.Id);
    }

    [Fact]
    public async Task Dada_SimulacaoRoteada_Quando_CapturaLead_Entao_NaoEmiteProposta()
    {
        const string codigoIbge = "2222222";
        await PrepararCenarioAsync(codigoIbge);
        _cliente.DefaultRequestHeaders.Authorization = null;
        var criar = await _cliente.PostAsJsonAsync("/api/publico/simulacoes",
            new CriarSimulacaoPublicaRequest(500m, null, TipoLigacao.Monofasica,
                PerfilImovel.Residencial, codigoIbge, TipoTelhado.Ceramico, 1000m, true));
        var simulacao = await criar.Content.ReadFromJsonAsync<SimulacaoPublicaResponse>();

        using var formulario = FormularioLead(consentimento: true);
        var response = await _cliente.PostAsync($"/api/publico/simulacoes/{simulacao!.Id}/lead", formulario);
        var resultado = await response.Content.ReadFromJsonAsync<CapturarLeadPublicoResponse>();

        Assert.Equal(nameof(DesfechoCapturaLead.RoteadoParaHumano), resultado!.Desfecho);
        using var escopo = _factory.Services.CreateScope();
        var contexto = escopo.ServiceProvider.GetRequiredService<SolarESDbContext>();
        // Idem: sem Proposta pra esta simulacao, nenhum envio pode ter sido
        // enfileirado (ver comentario equivalente no teste de calibracao pendente).
        Assert.DoesNotContain(await contexto.Propostas.ToListAsync(), proposta => proposta.SimulacaoId == simulacao.Id);
    }

    [Fact]
    public async Task Dada_VersaoConfirmadaGravadaNaSimulacao_Quando_CapturaLead_Entao_EmiteProposta()
    {
        const string codigoIbge = "1111111";
        await PrepararCenarioAsync(codigoIbge);
        Guid simulacaoId;
        using (var escopo = _factory.Services.CreateScope())
        {
            var contexto = escopo.ServiceProvider.GetRequiredService<SolarESDbContext>();
            var atual = await contexto.ConfiguracoesVersao.SingleAsync(c => c.Status == StatusConfiguracaoVersao.Publicada);
            atual.Arquivar();
            var confirmada = ConfiguracaoVersao.CriarRascunho(atual.Numero + 1, ConfiguracaoConfirmada(), "Teste");
            confirmada.Publicar(Guid.NewGuid(), DateTimeOffset.UtcNow);
            contexto.ConfiguracoesVersao.Add(confirmada);
            await contexto.SaveChangesAsync();
        }
        _cliente.DefaultRequestHeaders.Authorization = null;
        var criar = await _cliente.PostAsJsonAsync("/api/publico/simulacoes",
            new CriarSimulacaoPublicaRequest(500m, null, TipoLigacao.Monofasica,
                PerfilImovel.Residencial, codigoIbge, TipoTelhado.Ceramico, 1000m, false));
        simulacaoId = (await criar.Content.ReadFromJsonAsync<SimulacaoPublicaResponse>())!.Id;

        using var formulario = FormularioLead(consentimento: true);
        var response = await _cliente.PostAsync($"/api/publico/simulacoes/{simulacaoId}/lead", formulario);
        var resultado = await response.Content.ReadFromJsonAsync<CapturarLeadPublicoResponse>();

        Assert.Equal(nameof(DesfechoCapturaLead.PropostaEmitida), resultado!.Desfecho);
        using var verificacao = _factory.Services.CreateScope();
        var banco = verificacao.ServiceProvider.GetRequiredService<SolarESDbContext>();
        var propostaCriada = Assert.Single(await banco.Propostas.Where(proposta => proposta.SimulacaoId == simulacaoId).ToListAsync());
        var pdfGerado = false;
        for (var tentativa = 0; tentativa < 25 && !pdfGerado; tentativa++)
        {
            await Task.Delay(200);
            await banco.Entry(propostaCriada).ReloadAsync();
            pdfGerado = propostaCriada.ArquivoPdfUrl is not null;
        }
        Assert.True(pdfGerado, "O job assíncrono não gerou o PDF da proposta.");

        // T25.3: captura com PropostaEmitida enfileira o envio automatico pelo canal
        // preferido do lead, como continuacao da geracao do PDF.
        var envioEnviado = false;
        EnvioProposta? envioCriado = null;
        for (var tentativa = 0; tentativa < 25 && !envioEnviado; tentativa++)
        {
            await Task.Delay(200);
            envioCriado = await banco.EnviosProposta.SingleOrDefaultAsync(e => e.PropostaId == propostaCriada.Id);
            envioEnviado = envioCriado?.Status == StatusEnvioProposta.Enviado;
        }
        Assert.True(envioEnviado, "O envio automatico nao foi concluido a tempo.");
        Assert.Equal(CanalEnvio.Email, envioCriado!.Canal);
        Assert.Equal("maria@exemplo.com", envioCriado.Destino);
        var propostaAtualizada = await banco.Propostas.SingleAsync(p => p.Id == propostaCriada.Id);
        Assert.NotNull(propostaAtualizada.EnviadaEm);

        var publicada = await banco.ConfiguracoesVersao.SingleAsync(c => c.Status == StatusConfiguracaoVersao.Publicada);
        publicada.Arquivar();
        var baseline = ConfiguracaoVersao.CriarRascunho(publicada.Numero + 1, ConfiguracaoCalculoBaseline.Criar(), "Teste");
        baseline.Publicar(Guid.NewGuid(), DateTimeOffset.UtcNow);
        banco.ConfiguracoesVersao.Add(baseline);
        await banco.SaveChangesAsync();
    }

    [Fact]
    public async Task Dado_PdfValido_Quando_AnexaConta_Entao_PersisteEBaixaSomenteAutenticado()
    {
        const string codigoIbge = "1212121";
        await PrepararCenarioAsync(codigoIbge);
        _cliente.DefaultRequestHeaders.Authorization = null;
        var criar = await _cliente.PostAsJsonAsync("/api/publico/simulacoes",
            new CriarSimulacaoPublicaRequest(500m, null, TipoLigacao.Monofasica,
                PerfilImovel.Residencial, codigoIbge, TipoTelhado.Ceramico, 1000m, false));
        var simulacao = await criar.Content.ReadFromJsonAsync<SimulacaoPublicaResponse>();
        var pdf = "%PDF-1.4 conta teste"u8.ToArray();

        using var formulario = FormularioLead(true, pdf, "conta.pdf", "application/pdf");
        Assert.Equal(HttpStatusCode.OK,
            (await _cliente.PostAsync($"/api/publico/simulacoes/{simulacao!.Id}/lead", formulario)).StatusCode);
        using var escopo = _factory.Services.CreateScope();
        var contexto = escopo.ServiceProvider.GetRequiredService<SolarESDbContext>();
        var lead = await contexto.Leads.SingleAsync(l => l.SimulacaoId == simulacao.Id);
        var anexo = await contexto.AnexosConta.SingleAsync(a => a.LeadId == lead.Id);
        Assert.Equal(TipoAnexoConta.Pdf, anexo.Tipo);
        Assert.Equal(pdf.Length, anexo.Tamanho);

        Assert.Equal(HttpStatusCode.Unauthorized, (await _cliente.GetAsync($"/api/leads/{lead.Id}/anexo")).StatusCode);
        await SolarESApiFactory.ClienteAutenticadoAsync(_cliente, SolarESApiFactory.VendedorEmail, SolarESApiFactory.VendedorSenha);
        var detalhe = await _cliente.GetFromJsonAsync<LeadResponse>($"/api/leads/{lead.Id}");
        Assert.True(detalhe!.PossuiAnexo);
        Assert.Contains(detalhe.Consentimentos, c =>
            c.Finalidade == FinalidadeConsentimento.ContatoComercial
            && c.VersaoTexto == "contato-comercial-v1");
        var download = await _cliente.GetAsync($"/api/leads/{lead.Id}/anexo");
        Assert.Equal(HttpStatusCode.OK, download.StatusCode);
        Assert.Equal(pdf, await download.Content.ReadAsByteArrayAsync());
    }

    [Fact]
    public async Task Dado_ExecutavelRenomeado_Quando_AnexaComoPdf_Entao_RecusaSemCriarLead()
    {
        const string codigoIbge = "1313131";
        await PrepararCenarioAsync(codigoIbge);
        _cliente.DefaultRequestHeaders.Authorization = null;
        var criar = await _cliente.PostAsJsonAsync("/api/publico/simulacoes",
            new CriarSimulacaoPublicaRequest(500m, null, TipoLigacao.Monofasica,
                PerfilImovel.Residencial, codigoIbge, TipoTelhado.Ceramico, 1000m, false));
        var simulacao = await criar.Content.ReadFromJsonAsync<SimulacaoPublicaResponse>();

        using var formulario = FormularioLead(true, "MZ executavel"u8.ToArray(), "conta.pdf", "application/pdf");
        var response = await _cliente.PostAsync($"/api/publico/simulacoes/{simulacao!.Id}/lead", formulario);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        using var escopo = _factory.Services.CreateScope();
        var contexto = escopo.ServiceProvider.GetRequiredService<SolarESDbContext>();
        Assert.DoesNotContain(await contexto.Leads.ToListAsync(), lead => lead.SimulacaoId == simulacao.Id);
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

    private static MultipartFormDataContent FormularioLead(bool consentimento, byte[]? anexo = null,
        string nomeArquivo = "conta.pdf", string tipoConteudo = "application/pdf",
        string versaoTexto = "contato-comercial-v1")
    {
        var formulario = new MultipartFormDataContent();
        formulario.Add(new StringContent("Maria Silva"), "Nome");
        formulario.Add(new StringContent("27999999999"), "Telefone");
        formulario.Add(new StringContent("maria@exemplo.com"), "Email");
        formulario.Add(new StringContent(nameof(CanalPreferido.Email)), "CanalPreferido");
        if (consentimento)
            formulario.Add(new StringContent(nameof(FinalidadeConsentimento.ContatoComercial)), "FinalidadesAceitas");
        formulario.Add(new StringContent(versaoTexto), "VersaoTexto");
        if (anexo is not null)
        {
            var arquivo = new ByteArrayContent(anexo);
            arquivo.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(tipoConteudo);
            formulario.Add(arquivo, "Anexo", nomeArquivo);
        }
        return formulario;
    }

    private async Task PrepararCenarioAsync(string codigoIbge)
    {
        await SolarESApiFactory.ClienteAutenticadoAsync(
            _cliente, SolarESApiFactory.DonoEmail, SolarESApiFactory.DonoSenha);
        var sufixo = Guid.NewGuid().ToString("N")[..8];
        await _cliente.PostAsJsonAsync("/api/catalogo/modulos-fotovoltaicos", new ModuloFotovoltaicoRequest(
            "Fabricante", $"Modulo-{sufixo}", 550, 1134, 2278, 21.3m, false, true));
        await _cliente.PostAsJsonAsync("/api/catalogo/inversores", new InversorRequest(
            "Fabricante", $"Inversor-{sufixo}", 10000, 2, TipoInversor.String, true));
        var distribuidoraHttp = await _cliente.PostAsJsonAsync("/api/tarifas/distribuidoras",
            new DistribuidoraRequest("Distribuidora Publica", $"DP-{sufixo}", true));
        var distribuidora = await distribuidoraHttp.Content.ReadFromJsonAsync<DistribuidoraResponse>();
        await _cliente.PostAsJsonAsync("/api/tarifas/municipios", new MunicipioHspRequest(
            codigoIbge, "Municipio Publico", -20m, -40m, 200m, distribuidora!.Id,
            Enumerable.Repeat(5m, 12).ToList(), "Teste"));
        await _cliente.PostAsJsonAsync("/api/tarifas/vigentes", new TarifaVigenteRequest(
            distribuidora.Id, Subgrupo.B1, .35m, .40m, .21m, .17m, .0925m,
            DateTimeOffset.UtcNow.AddDays(-1), null, "Teste", "Teste"));
        await _cliente.PostAsJsonAsync("/api/precificacao/faixas-preco", new FaixaPrecoRequest(
            0m, 500m, 3.5m, "Residencial", false, DateTimeOffset.UtcNow.AddDays(-1)));

        using var escopo = _factory.Services.CreateScope();
        var contexto = escopo.ServiceProvider.GetRequiredService<SolarESDbContext>();
        if (!await contexto.ConfiguracoesVersao.AnyAsync(c => c.Status == StatusConfiguracaoVersao.Publicada))
        {
            var agora = DateTimeOffset.UtcNow;
            var versao = ConfiguracaoVersao.CriarRascunho(1, ConfiguracaoCalculoBaseline.Criar(), "Teste");
            versao.Publicar(Guid.NewGuid(), agora);
            contexto.ConfiguracoesVersao.Add(versao);
            await contexto.SaveChangesAsync();
        }
    }
}
