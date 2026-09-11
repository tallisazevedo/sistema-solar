using System.Text.Json;
using SolarES.Aplicacao.Compartilhado;
using SolarES.Aplicacao.Configuracao;
using SolarES.Dominio.Catalogo;
using SolarES.Dominio.Configuracao;
using SolarES.Dominio.Precificacao;
using SolarES.Dominio.Simulacao;
using SolarES.Dominio.Tarifas;
using SimulacaoEntidade = SolarES.Dominio.Simulacao.Simulacao;

namespace SolarES.Aplicacao.Simulacoes;

public sealed class SimulacaoAppService(
    ISimulacaoRepository simulacaoRepositorio,
    IConfiguracaoVersaoRepository configuracaoRepositorio,
    IRepositorioCrud<ModuloFotovoltaico> modulosRepositorio,
    IRepositorioCrud<Inversor> inversoresRepositorio,
    IRepositorioCrud<FaixaPreco> faixasPrecoRepositorio,
    IRepositorioCrud<MunicipioHsp> municipiosRepositorio,
    IRepositorioCrud<TarifaVigente> tarifasRepositorio,
    TimeProvider relogio)
{
    public async Task<SimulacaoEntidade> CriarAsync(
        EntradaSimulacao entrada,
        CancellationToken ct,
        OrigemSimulacao origem = OrigemSimulacao.Interna)
    {
        var configuracaoVersao = await configuracaoRepositorio.ObterPublicadaAtivaAsync(ct)
            ?? throw new InvalidOperationException("Nao ha versao de configuracao publicada; nao e possivel simular.");

        var municipios = await municipiosRepositorio.ListarAsync(ct);
        var municipio = municipios.FirstOrDefault(m => m.CodigoIbge == entrada.MunicipioCodigoIbge)
            ?? throw new InvalidOperationException($"Municipio '{entrada.MunicipioCodigoIbge}' nao encontrado.");

        var hoje = relogio.GetUtcNow();
        var tarifas = await tarifasRepositorio.ListarAsync(ct);
        var tarifa = tarifas
            .Where(t => t.DistribuidoraId == municipio.DistribuidoraId
                && t.Subgrupo == entrada.Subgrupo
                && t.VigenciaInicio <= hoje
                && (t.VigenciaFim == null || t.VigenciaFim >= hoje))
            .OrderByDescending(t => t.VigenciaInicio)
            .FirstOrDefault()
            ?? throw new InvalidOperationException(
                $"Nenhuma tarifa vigente para a distribuidora do municipio '{entrada.MunicipioCodigoIbge}' no subgrupo {entrada.Subgrupo}.");

        // Composicao de tarifaCheia = TE + TUSD, com tributos aplicados por gross-up
        // aditivo simples (docs/02 diz "TE + TUSD e tributos" sem formula exata;
        // decisao registrada no plano da T19).
        var tarifaCheia = (tarifa.TarifaTe + tarifa.TarifaTusd) * (1 + tarifa.AliquotaIcms + tarifa.AliquotaPisCofins);

        var modulos = await modulosRepositorio.ListarAsync(ct);
        var modulo = modulos
            .Where(m => m.Ativo)
            .OrderByDescending(m => m.PotenciaW)
            .FirstOrDefault()
            ?? throw new InvalidOperationException("Nenhum modulo fotovoltaico ativo no catalogo.");

        var inversores = (await inversoresRepositorio.ListarAsync(ct)).Where(i => i.Ativo).ToList();
        var faixasPreco = await faixasPrecoRepositorio.ListarAsync(ct);

        var resultado = MotorSimulacao.Simular(
            entrada,
            configuracaoVersao.Payload,
            municipio.HspPorMes.Average(),
            municipio.HspPorMes,
            municipio.DistanciaMarKm,
            modulo,
            inversores,
            faixasPreco,
            tarifaCheia,
            tarifa.ValorFioBPorKwh,
            hoje.Year);

        var simulacao = new SimulacaoEntidade
        {
            Id = Guid.NewGuid(),
            ConfiguracaoVersaoId = configuracaoVersao.Id,
            Origem = origem,
            EntradasSnapshot = JsonSerializer.Serialize(entrada),
            ResultadoSnapshot = JsonSerializer.Serialize(resultado),
            PotenciaKwp = resultado.PotenciaInstaladaKwp,
            QuantidadeModulos = resultado.QuantidadeModulos,
            Capex = resultado.Capex,
            EconomiaMensalAno1 = resultado.EconomiaMensalAno1,
            PaybackMeses = resultado.PaybackMesesSimples,
            Tir = resultado.Tir,
            Vpl = resultado.Vpl,
            CoberturaPercentual = resultado.CoberturaPercentual,
            RoteadaParaHumano = resultado.RoteadaParaHumano,
            MotivoRoteamento = resultado.MotivoRoteamento,
            CriadoEm = hoje,
            AtualizadoEm = hoje,
        };

        simulacaoRepositorio.Adicionar(simulacao);
        await simulacaoRepositorio.SalvarAlteracoesAsync(ct);

        return simulacao;
    }

    public Task<IReadOnlyList<SimulacaoEntidade>> ListarAsync(CancellationToken ct) =>
        simulacaoRepositorio.ListarMaisRecentesAsync(ct);

    public Task<SimulacaoEntidade?> ObterPorIdAsync(Guid id, CancellationToken ct) =>
        simulacaoRepositorio.ObterPorIdAsync(id, ct);

    public async Task<SimulacaoPublicaResultado> CriarPublicaAsync(EntradaSimulacao entrada, CancellationToken ct)
    {
        var simulacao = await CriarAsync(entrada, ct, OrigemSimulacao.Landing);
        return await MontarResultadoPublicoAsync(simulacao, ct);
    }

    public async Task<SimulacaoPublicaResultado?> ObterPublicaPorIdAsync(Guid id, CancellationToken ct)
    {
        var simulacao = await simulacaoRepositorio.ObterPorIdAsync(id, ct);
        return simulacao is null || simulacao.Origem != OrigemSimulacao.Landing
            ? null
            : await MontarResultadoPublicoAsync(simulacao, ct);
    }

    private async Task<SimulacaoPublicaResultado> MontarResultadoPublicoAsync(
        SimulacaoEntidade simulacao,
        CancellationToken ct)
    {
        var versao = await configuracaoRepositorio.ObterPorIdAsync(simulacao.ConfiguracaoVersaoId, ct)
            ?? throw new InvalidOperationException("Versao de configuracao da simulacao nao encontrada.");
        return new SimulacaoPublicaResultado(simulacao, versao.Payload.PossuiPremissaProvisoria());
    }
}

public sealed record SimulacaoPublicaResultado(SimulacaoEntidade Simulacao, bool CalibracaoPendente);
