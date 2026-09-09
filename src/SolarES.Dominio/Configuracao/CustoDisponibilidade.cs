using SolarES.Dominio.Simulacao;

namespace SolarES.Dominio.Configuracao;

/// <summary>
/// Consumo minimo faturado pela distribuidora mesmo que o sistema gere 100% da
/// energia. Nao e compensavel (docs/02).
/// </summary>
public sealed record CustoDisponibilidade(decimal Monofasica, decimal Bifasica, decimal Trifasica)
{
    public decimal ParaLigacao(TipoLigacao ligacao) => ligacao switch
    {
        TipoLigacao.Monofasica => Monofasica,
        TipoLigacao.Bifasica => Bifasica,
        TipoLigacao.Trifasica => Trifasica,
        _ => throw new ArgumentOutOfRangeException(nameof(ligacao), ligacao, "Tipo de ligacao desconhecido."),
    };
}
