namespace SolarES.Dominio;

internal static class MatematicaFinanceira
{
    /// <summary>Potencia com expoente inteiro em decimal (juros compostos, degradacao) — evita o double de Math.Pow.</summary>
    public static decimal Potencia(decimal @base, int expoente)
    {
        var resultado = 1m;
        for (var i = 0; i < expoente; i++)
        {
            resultado *= @base;
        }

        return resultado;
    }
}
