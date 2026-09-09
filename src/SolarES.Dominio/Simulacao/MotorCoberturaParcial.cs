using SolarES.Dominio.Catalogo;

namespace SolarES.Dominio.Simulacao;

/// <summary>
/// Cobertura parcial por area (docs/02, regra de negocio 3): quando a area
/// disponivel nao comporta o sistema ideal, devolve o percentual de cobertura
/// alcancavel em vez de reduzir o sistema silenciosamente.
/// </summary>
public static class MotorCoberturaParcial
{
    public static ResultadoCobertura Ajustar(ResultadoDimensionamento dimensionamento, ModuloFotovoltaico modulo, decimal areaDisponivelM2)
    {
        if (dimensionamento.QuantidadeModulos == 0)
        {
            return new ResultadoCobertura(0, 0m, 100m);
        }

        var areaModuloM2 = (modulo.LarguraMm / 1000m) * (modulo.AlturaMm / 1000m);
        var quantidadeMaxPorArea = (int)Math.Floor(areaDisponivelM2 / areaModuloM2);
        var quantidadeFinal = Math.Min(dimensionamento.QuantidadeModulos, quantidadeMaxPorArea);

        var potenciaInstaladaFinalKwp = quantidadeFinal * modulo.PotenciaW / 1000m;
        var coberturaPercentual = (decimal)quantidadeFinal / dimensionamento.QuantidadeModulos * 100m;

        return new ResultadoCobertura(quantidadeFinal, potenciaInstaladaFinalKwp, coberturaPercentual);
    }
}
