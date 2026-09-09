using SolarES.Dominio.Configuracao;

namespace SolarES.Dominio.Simulacao;

/// <summary>
/// Kit litoral (docs/02, regra de negocio 2). ConfiguracaoKitLitoral carrega dois
/// campos e este motor usa os dois, em OU: a lista de municipios e o override
/// explicito do dono (garante um municipio especifico independente do geodado); o
/// raio e a regra geral, aplicada sobre MunicipioHsp.DistanciaMarKm (T06) para
/// qualquer municipio que nao esteja na lista. Decisao de leitura do docs/02
/// registrada no plano da T13 -- revisavel no admin (T18).
/// </summary>
public static class MotorKitLitoral
{
    public static bool Aplica(ConfiguracaoCalculo configuracao, string municipioCodigoIbge, decimal distanciaMarKm)
    {
        var kitLitoral = configuracao.KitLitoral.Valor;

        return kitLitoral.MunicipiosCodigoIbge.Contains(municipioCodigoIbge)
            || distanciaMarKm <= kitLitoral.RaioKm;
    }
}
