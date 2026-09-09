using Microsoft.EntityFrameworkCore;
using SolarES.Infraestrutura.Persistencia;
using SolarES.Infraestrutura.Persistencia.Seeds;

namespace SolarES.Infraestrutura.Tests;

public class TarifaVigenteSeederTests
{
    private static async Task<SolarESDbContext> CriarContextoComDistribuidorasAsync()
    {
        var options = new DbContextOptionsBuilder<SolarESDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var contexto = new SolarESDbContext(options);
        await MunicipioHspSeeder.SeedAsync(contexto, CancellationToken.None);
        return contexto;
    }

    [Fact]
    public async Task SeedAsync_ComDistribuidorasExistentes_InsereAsSeisTarifas()
    {
        await using var contexto = await CriarContextoComDistribuidorasAsync();

        await TarifaVigenteSeeder.SeedAsync(contexto, CancellationToken.None);

        Assert.Equal(6, await contexto.TarifasVigentes.CountAsync());
    }

    [Fact]
    public async Task SeedAsync_ChamadoDuasVezes_NaoDuplica()
    {
        await using var contexto = await CriarContextoComDistribuidorasAsync();

        await TarifaVigenteSeeder.SeedAsync(contexto, CancellationToken.None);
        await TarifaVigenteSeeder.SeedAsync(contexto, CancellationToken.None);

        Assert.Equal(6, await contexto.TarifasVigentes.CountAsync());
    }

    [Fact]
    public async Task SeedAsync_TodaLinha_TemResolucaoHomologatoriaEVigenciaCoerente()
    {
        await using var contexto = await CriarContextoComDistribuidorasAsync();
        await TarifaVigenteSeeder.SeedAsync(contexto, CancellationToken.None);

        var tarifas = await contexto.TarifasVigentes.ToListAsync();

        Assert.All(tarifas, t => Assert.False(string.IsNullOrWhiteSpace(t.ResolucaoHomologatoria)));
        Assert.All(tarifas, t => Assert.True(t.VigenciaFim is null || t.VigenciaInicio < t.VigenciaFim));
    }

    [Fact]
    public async Task SeedAsync_ValorFioBPorKwh_EhSempreMenorQueTarifaTusd()
    {
        await using var contexto = await CriarContextoComDistribuidorasAsync();
        await TarifaVigenteSeeder.SeedAsync(contexto, CancellationToken.None);

        var tarifas = await contexto.TarifasVigentes.ToListAsync();

        Assert.All(tarifas, t => Assert.True(t.ValorFioBPorKwh < t.TarifaTusd));
    }
}
