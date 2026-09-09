namespace SolarES.Dominio.Precificacao;

/// <summary>
/// Capex a partir da faixa de preco aplicavel (docs/02, "preco proprio" do kit
/// litoral; docs/04, FaixaPreco.KitLitoral).
/// </summary>
public static class MotorPrecificacao
{
    public static decimal? CalcularCapex(decimal potenciaInstaladaKwp, bool kitLitoral, IReadOnlyList<FaixaPreco> catalogoFaixas)
    {
        var faixa = catalogoFaixas.FirstOrDefault(f =>
            f.KitLitoral == kitLitoral &&
            potenciaInstaladaKwp >= f.KwpMinimo &&
            potenciaInstaladaKwp <= f.KwpMaximo);

        return faixa is null ? null : potenciaInstaladaKwp * 1000m * faixa.PrecoPorWp;
    }
}
