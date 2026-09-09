using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SolarES.Dominio.Simulacao;
using SolarES.Dominio.Tarifas;

namespace SolarES.Infraestrutura.Persistencia.Seeds;

/// <summary>
/// Seed das tarifas vigentes de EDP ES e ELFSM, subgrupos B1/B2/B3 (docs/04, docs/05
/// T07). Pressupoe que as duas Distribuidora ja existem — rode
/// <see cref="MunicipioHspSeeder.SeedAsync"/> antes. Idempotente por
/// (DistribuidoraId, Subgrupo, VigenciaInicio).
///
/// Fontes dos dados embutidos em tarifas-es.json:
/// - TarifaTe, TarifaTusd: dataset ANEEL "tarifas-distribuidoras-energia-eletrica",
///   filtrado para DscBaseTarifaria="Tarifa de Aplicacao" (tarifa efetivamente
///   cobrada) e DscDetalhe="Nao se aplica" (exclui a linha SCEE, que ja embute o
///   desconto de compensacao -- o motor calcula isso sozinho a partir do Fio B, T10).
/// - ValorFioBPorKwh: dataset ANEEL "componentes-tarifarias-2026",
///   DscComponenteTarifario="TUSD_FioB". A ELFSM nao possui linhas nesse dataset;
///   usa-se o valor da EDP ES como proxy (mesma metodologia nacional ANEEL para
///   baixa tensao -- o valor e identico entre B1/B2/B3 na propria EDP ES, indicando
///   que nao varia por classe de consumidor). Ver Fonte de cada linha ELFSM para o
///   detalhe -- revisar quando a REH 3.519/2025 da ELFSM puder ser lida diretamente
///   (cedoc.aneel.gov.br bloqueado por desafio Cloudflare no momento da coleta).
/// - AliquotaIcms: 17% fixo (aliquota interna do ES, sem escalonamento por faixa de
///   consumo). AliquotaPisCofins: 9,25% (1,65% + 7,6%, aliquota combinada padrao
///   nao-cumulativa) -- na pratica varia mes a mes por distribuidora ("calculo por
///   dentro" sobre creditos/debitos); usado como referencia, nao como valor
///   homologado imutavel. Nenhum dos dois vem da ANEEL: a tarifa homologada e
///   publicada sem tributos.
/// </summary>
public static class TarifaVigenteSeeder
{
    private static readonly JsonSerializerOptions OpcoesJson = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    public static async Task SeedAsync(SolarESDbContext contexto, CancellationToken ct)
    {
        var idPorSigla = await contexto.Distribuidoras.ToDictionaryAsync(d => d.SiglaAneel, d => d.Id, ct);

        var registros = LerRegistrosEmbutidos();
        var existentes = await contexto.TarifasVigentes
            .Select(t => new { t.DistribuidoraId, t.Subgrupo, t.VigenciaInicio })
            .ToListAsync(ct);
        var chavesExistentes = existentes
            .Select(e => (e.DistribuidoraId, e.Subgrupo, e.VigenciaInicio))
            .ToHashSet();

        foreach (var registro in registros)
        {
            if (!idPorSigla.TryGetValue(registro.SiglaDistribuidora, out var distribuidoraId))
            {
                throw new InvalidOperationException(
                    $"Distribuidora '{registro.SiglaDistribuidora}' nao encontrada. Rode MunicipioHspSeeder.SeedAsync antes.");
            }

            var subgrupo = Enum.Parse<Subgrupo>(registro.Subgrupo);
            if (chavesExistentes.Contains((distribuidoraId, subgrupo, registro.VigenciaInicio)))
            {
                continue;
            }

            contexto.TarifasVigentes.Add(new TarifaVigente
            {
                Id = Guid.NewGuid(),
                DistribuidoraId = distribuidoraId,
                Subgrupo = subgrupo,
                TarifaTe = registro.TarifaTe,
                TarifaTusd = registro.TarifaTusd,
                ValorFioBPorKwh = registro.ValorFioBPorKwh,
                AliquotaIcms = registro.AliquotaIcms,
                AliquotaPisCofins = registro.AliquotaPisCofins,
                VigenciaInicio = registro.VigenciaInicio,
                VigenciaFim = registro.VigenciaFim,
                ResolucaoHomologatoria = registro.ResolucaoHomologatoria,
                Fonte = registro.Fonte,
            });
        }

        await contexto.SaveChangesAsync(ct);
    }

    private static List<TarifaVigenteSeedRegistro> LerRegistrosEmbutidos()
    {
        var assembly = typeof(TarifaVigenteSeeder).Assembly;
        const string recurso = "SolarES.Infraestrutura.Persistencia.Seeds.tarifas-es.json";

        using var stream = assembly.GetManifestResourceStream(recurso)
            ?? throw new InvalidOperationException($"Recurso embutido nao encontrado: {recurso}");

        return JsonSerializer.Deserialize<List<TarifaVigenteSeedRegistro>>(stream, OpcoesJson)
            ?? throw new InvalidOperationException("Nao foi possivel ler tarifas-es.json.");
    }
}
