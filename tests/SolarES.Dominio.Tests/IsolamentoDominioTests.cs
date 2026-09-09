using System.Xml.Linq;

namespace SolarES.Dominio.Tests;

public class IsolamentoDominioTests
{
    private static readonly string CaminhoCsproj = LocalizarCsprojDominio();

    [Fact]
    public void Dominio_NaoTemPackageReference()
    {
        var documento = XDocument.Load(CaminhoCsproj);

        var pacotes = documento.Descendants("PackageReference")
            .Select(elemento => elemento.Attribute("Include")?.Value)
            .ToList();

        Assert.Empty(pacotes);
    }

    [Fact]
    public void Dominio_NaoReferenciaAplicacaoInfraestruturaOuApi()
    {
        var documento = XDocument.Load(CaminhoCsproj);

        var referencias = documento.Descendants("ProjectReference")
            .Select(elemento => elemento.Attribute("Include")?.Value)
            .ToList();

        Assert.Empty(referencias);
    }

    private static string LocalizarCsprojDominio()
    {
        var diretorio = new DirectoryInfo(AppContext.BaseDirectory);
        while (diretorio is not null && !File.Exists(Path.Combine(diretorio.FullName, "SolarES.slnx")))
        {
            diretorio = diretorio.Parent;
        }

        if (diretorio is null)
        {
            throw new InvalidOperationException("Nao foi possivel localizar a raiz do repositorio (SolarES.slnx).");
        }

        return Path.Combine(diretorio.FullName, "src", "SolarES.Dominio", "SolarES.Dominio.csproj");
    }
}
