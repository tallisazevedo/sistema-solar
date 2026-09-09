using Microsoft.EntityFrameworkCore;
using SolarES.Infraestrutura.Persistencia;
using SolarES.Infraestrutura.Persistencia.Seeds;

namespace SolarES.Infraestrutura.Tests;

public class MunicipioHspSeederTests
{
    private static SolarESDbContext CriarContexto()
    {
        var options = new DbContextOptionsBuilder<SolarESDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new SolarESDbContext(options);
    }

    [Fact]
    public async Task SeedAsync_EmBaseVazia_InsereDuasDistribuidorasESetentaEOitoMunicipios()
    {
        await using var contexto = CriarContexto();

        await MunicipioHspSeeder.SeedAsync(contexto, CancellationToken.None);

        Assert.Equal(2, await contexto.Distribuidoras.CountAsync());
        Assert.Equal(78, await contexto.MunicipiosHsp.CountAsync());
    }

    [Fact]
    public async Task SeedAsync_ChamadoDuasVezes_NaoDuplica()
    {
        await using var contexto = CriarContexto();

        await MunicipioHspSeeder.SeedAsync(contexto, CancellationToken.None);
        await MunicipioHspSeeder.SeedAsync(contexto, CancellationToken.None);

        Assert.Equal(2, await contexto.Distribuidoras.CountAsync());
        Assert.Equal(78, await contexto.MunicipiosHsp.CountAsync());
    }

    [Fact]
    public async Task SeedAsync_TodosOsSetentaEOitoCodigosIbgeEstaoPresentes()
    {
        await using var contexto = CriarContexto();

        await MunicipioHspSeeder.SeedAsync(contexto, CancellationToken.None);

        var codigos = await contexto.MunicipiosHsp.Select(m => m.CodigoIbge).ToListAsync();

        Assert.Equal(78, codigos.Distinct().Count());
        Assert.All(codigos, codigo => Assert.Matches("^32\\d{5}$", codigo));
    }

    [Fact]
    public async Task SeedAsync_NaoSobrescreveEdicaoDoAdmin()
    {
        await using var contexto = CriarContexto();
        await MunicipioHspSeeder.SeedAsync(contexto, CancellationToken.None);

        var municipio = await contexto.MunicipiosHsp.FirstAsync(m => m.CodigoIbge == "3205309");
        var hspEditadoPeloAdmin = new List<decimal> { 9m, 9m, 9m, 9m, 9m, 9m, 9m, 9m, 9m, 9m, 9m, 9m };
        municipio.HspPorMes = hspEditadoPeloAdmin;
        await contexto.SaveChangesAsync();

        await MunicipioHspSeeder.SeedAsync(contexto, CancellationToken.None);

        var municipioAposReseed = await contexto.MunicipiosHsp.FirstAsync(m => m.CodigoIbge == "3205309");
        Assert.Equal(hspEditadoPeloAdmin, municipioAposReseed.HspPorMes);
    }
}
