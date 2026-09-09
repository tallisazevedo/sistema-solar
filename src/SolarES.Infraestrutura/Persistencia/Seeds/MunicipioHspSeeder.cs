using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SolarES.Dominio.Tarifas;

namespace SolarES.Infraestrutura.Persistencia.Seeds;

/// <summary>
/// Seed dos 78 municipios do ES (docs/04, docs/05 T06). Idempotente por
/// CodigoIbge/SiglaAneel: nunca sobrescreve uma linha ja existente, porque o
/// dataset e "editavel no admin" depois do seed inicial (docs/04).
///
/// Fontes dos dados embutidos em municipios-es.json:
/// - CodigoIbge, nome, coordenadas: API oficial do IBGE (localidades + malhas),
///   centroide calculado da malha municipal.
/// - HspPorMes: NASA POWER (ALLSKY_SFC_SW_DWN, climatologia 2001-2020, community=RE)
///   no centroide de cada municipio. O Atlas Brasileiro de Energia Solar
///   (INPE/LABREN, docs/02) era a fonte preferida, mas o servidor do INPE estava
///   inalcancavel no momento da coleta; os valores foram cruzados contra o estudo
///   "A Energia Solar no Espirito Santo" (ASPE/INPE/LabSolar, 2013) e batem com a
///   faixa publicada por microrregiao. Revisar com INPE/LABREN direto quando o
///   servidor voltar a responder.
/// - DistanciaMarKm: haversine do centroide ao municipio costeiro mais proximo.
/// - SiglaDistribuidora: EDP Espirito Santo ou ELFSM (Empresa Luz e Forca Santa
///   Maria), por area de concessao (portal.elfsm.com.br). Modelo e por municipio
///   inteiro; onde a concessao e por distrito (Santa Teresa), o municipio ficou
///   com quem atende a sede.
/// </summary>
public static class MunicipioHspSeeder
{
    private static readonly (string Sigla, string Nome)[] Distribuidoras =
    [
        ("EDP ES", "EDP Espirito Santo Distribuicao de Energia S.A."),
        ("ELFSM", "Empresa Luz e Forca Santa Maria S/A"),
    ];

    private static readonly JsonSerializerOptions OpcoesJson = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    public static async Task SeedAsync(SolarESDbContext contexto, CancellationToken ct)
    {
        var idPorSigla = await GarantirDistribuidorasAsync(contexto, ct);

        var registros = LerRegistrosEmbutidos();
        var codigosExistentes = await contexto.MunicipiosHsp
            .Select(m => m.CodigoIbge)
            .ToHashSetAsync(ct);

        foreach (var registro in registros)
        {
            if (codigosExistentes.Contains(registro.CodigoIbge))
            {
                continue;
            }

            contexto.MunicipiosHsp.Add(new MunicipioHsp
            {
                Id = Guid.NewGuid(),
                CodigoIbge = registro.CodigoIbge,
                Nome = registro.Nome,
                Latitude = registro.Latitude,
                Longitude = registro.Longitude,
                DistanciaMarKm = registro.DistanciaMarKm,
                DistribuidoraId = idPorSigla[registro.SiglaDistribuidora],
                HspPorMes = registro.HspPorMes,
                Fonte = "NASA POWER (ALLSKY_SFC_SW_DWN, climatologia 2001-2020); coordenadas IBGE.",
            });
        }

        await contexto.SaveChangesAsync(ct);
    }

    private static async Task<Dictionary<string, Guid>> GarantirDistribuidorasAsync(SolarESDbContext contexto, CancellationToken ct)
    {
        var existentes = await contexto.Distribuidoras.ToListAsync(ct);
        var resultado = new Dictionary<string, Guid>();

        foreach (var (sigla, nome) in Distribuidoras)
        {
            var existente = existentes.FirstOrDefault(d => d.SiglaAneel == sigla);
            if (existente is not null)
            {
                resultado[sigla] = existente.Id;
                continue;
            }

            var novaDistribuidora = new Distribuidora
            {
                Id = Guid.NewGuid(),
                Nome = nome,
                SiglaAneel = sigla,
                Ativa = true,
            };
            contexto.Distribuidoras.Add(novaDistribuidora);
            resultado[sigla] = novaDistribuidora.Id;
        }

        if (contexto.ChangeTracker.HasChanges())
        {
            await contexto.SaveChangesAsync(ct);
        }

        return resultado;
    }

    private static List<MunicipioSeedRegistro> LerRegistrosEmbutidos()
    {
        var assembly = typeof(MunicipioHspSeeder).Assembly;
        const string recurso = "SolarES.Infraestrutura.Persistencia.Seeds.municipios-es.json";

        using var stream = assembly.GetManifestResourceStream(recurso)
            ?? throw new InvalidOperationException($"Recurso embutido nao encontrado: {recurso}");

        return JsonSerializer.Deserialize<List<MunicipioSeedRegistro>>(stream, OpcoesJson)
            ?? throw new InvalidOperationException("Nao foi possivel ler municipios-es.json.");
    }
}
