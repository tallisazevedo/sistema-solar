namespace SolarES.Dominio.Simulacao;

public sealed class EntradaSimulacao
{
    public IReadOnlyList<decimal> HistoricoConsumoKwh { get; }
    public TipoLigacao TipoLigacao { get; }
    public Subgrupo Subgrupo { get; }
    public string MunicipioCodigoIbge { get; }
    public TipoTelhado TipoTelhado { get; }
    public decimal AreaDisponivelM2 { get; }

    public decimal ConsumoMedioMensal => HistoricoConsumoKwh.Average();

    public EntradaSimulacao(
        IReadOnlyList<decimal> historicoConsumoKwh,
        TipoLigacao tipoLigacao,
        Subgrupo subgrupo,
        string municipioCodigoIbge,
        TipoTelhado tipoTelhado,
        decimal areaDisponivelM2)
    {
        if (historicoConsumoKwh.Count != 12)
        {
            throw new ArgumentException(
                "HistoricoConsumoKwh precisa ter exatamente 12 meses — dimensionamento nunca usa um mes isolado.",
                nameof(historicoConsumoKwh));
        }

        HistoricoConsumoKwh = historicoConsumoKwh;
        TipoLigacao = tipoLigacao;
        Subgrupo = subgrupo;
        MunicipioCodigoIbge = municipioCodigoIbge;
        TipoTelhado = tipoTelhado;
        AreaDisponivelM2 = areaDisponivelM2;
    }
}
