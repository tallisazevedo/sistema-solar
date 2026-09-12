using System.Text.Json;
using Hangfire;
using Hangfire.InMemory;
using Microsoft.EntityFrameworkCore;
using SolarES.Aplicacao.Compartilhado;
using SolarES.Aplicacao.Propostas;
using SolarES.Aplicacao.Simulacoes;
using SolarES.Dominio.Catalogo;
using SolarES.Dominio.Configuracao;
using SolarES.Dominio.Precificacao;
using SolarES.Dominio.Proposta;
using SolarES.Dominio.Simulacao;
using SolarES.Dominio.Tarifas;
using SolarES.Infraestrutura.Persistencia;
using SolarES.Infraestrutura.Persistencia.Repositorios;
using PropostaEntidade = SolarES.Dominio.Proposta.Proposta;
using SimulacaoEntidade = SolarES.Dominio.Simulacao.Simulacao;

namespace SolarES.Aplicacao.Tests;

public sealed class RenovarPropostaTests
{
    private sealed class ArmazenamentoFalso : IArmazenamentoPdf
    {
        public Task<string> SalvarAsync(string numeroProposta, byte[] conteudo, CancellationToken ct) =>
            Task.FromResult(numeroProposta);
        public Task<byte[]?> LerAsync(string caminho, CancellationToken ct) => Task.FromResult<byte[]?>(null);
    }

    [Fact]
    public async Task Dada_PropostaVencida_Quando_Renova_Entao_UsaVersaoAtivaEPreservaOriginais()
    {
        var agora = new DateTimeOffset(2026, 9, 12, 12, 0, 0, TimeSpan.Zero);
        var contexto = new SolarESDbContext(new DbContextOptionsBuilder<SolarESDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        var versaoAntiga = ConfiguracaoVersao.CriarRascunho(1, ConfiguracaoCalculoBaseline.Criar());
        versaoAntiga.Publicar(Guid.NewGuid(), agora.AddMonths(-1));
        versaoAntiga.Arquivar();
        var versaoAtiva = ConfiguracaoVersao.CriarRascunho(2, ConfiguracaoCalculoBaseline.Criar());
        versaoAtiva.Publicar(Guid.NewGuid(), agora);
        var distribuidora = new Distribuidora { Id = Guid.NewGuid(), Nome = "EDP", SiglaAneel = "EDP", Ativa = true };
        var municipio = new MunicipioHsp
        {
            Id = Guid.NewGuid(),
            CodigoIbge = "3205309",
            Nome = "Vitoria",
            DistribuidoraId = distribuidora.Id,
            HspPorMes = Enumerable.Repeat(5m, 12).ToList(),
            DistanciaMarKm = 100,
            Fonte = "teste"
        };
        contexto.AddRange(versaoAntiga, versaoAtiva, distribuidora, municipio,
            new TarifaVigente
            {
                Id = Guid.NewGuid(),
                DistribuidoraId = distribuidora.Id,
                Subgrupo = Subgrupo.B1,
                TarifaTe = .4m,
                TarifaTusd = .4m,
                ValorFioBPorKwh = .2m,
                VigenciaInicio = agora.AddDays(-1),
                ResolucaoHomologatoria = "teste",
                Fonte = "teste"
            },
            new ModuloFotovoltaico
            {
                Id = Guid.NewGuid(),
                Fabricante = "A",
                Modelo = "M",
                PotenciaW = 550,
                LarguraMm = 1134,
                AlturaMm = 2278,
                EficienciaPercentual = 21,
                Ativo = true
            },
            new Inversor
            {
                Id = Guid.NewGuid(),
                Fabricante = "A",
                Modelo = "I",
                PotenciaW = 10000,
                QuantidadeMppt = 2,
                Tipo = TipoInversor.String,
                Ativo = true
            },
            new FaixaPreco
            {
                Id = Guid.NewGuid(),
                KwpMinimo = 0,
                KwpMaximo = 100,
                PrecoPorWp = 3.5m,
                TipoInstalacao = "Residencial",
                Vigencia = agora.AddDays(-1)
            });
        var entrada = new EntradaSimulacao(Enumerable.Repeat(500m, 12).ToList(), TipoLigacao.Monofasica,
            Subgrupo.B1, municipio.CodigoIbge, TipoTelhado.Ceramico, 100, false);
        var simulacaoOriginal = new SimulacaoEntidade
        {
            Id = Guid.NewGuid(),
            ConfiguracaoVersaoId = versaoAntiga.Id,
            EntradasSnapshot = JsonSerializer.Serialize(entrada),
            ResultadoSnapshot = "{}",
            CriadoEm = agora.AddMonths(-1),
            AtualizadoEm = agora.AddMonths(-1)
        };
        var propostaOriginal = new PropostaEntidade
        {
            Id = Guid.NewGuid(),
            SimulacaoId = simulacaoOriginal.Id,
            Numero = "PROP-ANTIGA",
            ConfiguracaoVersaoId = versaoAntiga.Id,
            ValidaAte = agora.AddDays(-1),
            Status = StatusProposta.Vencida,
            CriadoEm = agora.AddMonths(-1),
            AtualizadoEm = agora.AddMonths(-1)
        };
        contexto.AddRange(simulacaoOriginal, propostaOriginal);
        await contexto.SaveChangesAsync();
        var simulacoes = new EfSimulacaoRepository(contexto);
        var configuracoes = new ConfiguracaoVersaoRepository(contexto);
        var simulador = new SimulacaoAppService(simulacoes, configuracoes,
            new EfRepositorioCrud<ModuloFotovoltaico>(contexto), new EfRepositorioCrud<Inversor>(contexto),
            new EfRepositorioCrud<FaixaPreco>(contexto), new EfRepositorioCrud<MunicipioHsp>(contexto),
            new EfRepositorioCrud<TarifaVigente>(contexto), new FakeTimeProvider(agora));
        var servico = new PropostaAppService(new EfPropostaRepository(contexto), simulacoes, configuracoes,
            new ArmazenamentoFalso(), new BackgroundJobClient(new InMemoryStorage()), simulador,
            new EfExecutorTransacional(contexto), new FakeTimeProvider(agora));

        var entradasOriginais = simulacaoOriginal.EntradasSnapshot;
        var resultadoOriginal = simulacaoOriginal.ResultadoSnapshot;
        var numeroOriginal = propostaOriginal.Numero;

        var renovada = await servico.RenovarAsync(propostaOriginal.Id, Guid.NewGuid(), CancellationToken.None);

        Assert.NotEqual(propostaOriginal.Id, renovada.Id);
        Assert.NotEqual(numeroOriginal, renovada.Numero);
        Assert.Equal(versaoAtiva.Id, renovada.ConfiguracaoVersaoId);
        Assert.Equal(versaoAntiga.Id, propostaOriginal.ConfiguracaoVersaoId);
        Assert.Equal(StatusProposta.Vencida, propostaOriginal.Status);
        Assert.Equal(2, await contexto.Simulacoes.CountAsync());
        var novaSimulacao = await contexto.Simulacoes.SingleAsync(item => item.Id == renovada.SimulacaoId);
        Assert.Equal(entradasOriginais, novaSimulacao.EntradasSnapshot);
        Assert.Equal(entradasOriginais, simulacaoOriginal.EntradasSnapshot);
        Assert.Equal(resultadoOriginal, simulacaoOriginal.ResultadoSnapshot);
        Assert.Equal(numeroOriginal, propostaOriginal.Numero);
    }
}
