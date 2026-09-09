using System.Reflection;
using SolarES.Dominio.Premissas;

namespace SolarES.Dominio.Tests;

public class TiposDominioTests
{
    [Fact]
    public void NenhumTipoPublico_TemPropriedadeDouble()
    {
        var assembly = typeof(Premissa<int>).Assembly;

        var violacoes = assembly.GetTypes()
            .Where(tipo => tipo.IsPublic)
            .SelectMany(tipo => tipo.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            .Where(propriedade => propriedade.PropertyType == typeof(double) || propriedade.PropertyType == typeof(double?))
            .Select(propriedade => $"{propriedade.DeclaringType!.FullName}.{propriedade.Name}")
            .ToList();

        Assert.Empty(violacoes);
    }
}
