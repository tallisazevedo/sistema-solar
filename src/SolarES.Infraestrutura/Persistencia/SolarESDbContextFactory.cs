using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace SolarES.Infraestrutura.Persistencia;

/// <summary>
/// Usado só pelas ferramentas de design-time do EF Core (dotnet ef migrations add,
/// dotnet ef database update) — a Api monta o DbContext via DI em runtime normal.
/// </summary>
public sealed class SolarESDbContextFactory : IDesignTimeDbContextFactory<SolarESDbContext>
{
    public SolarESDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("ConnectionStrings__SolarES")
            ?? "Host=localhost;Port=5432;Database=solares;Username=postgres;Password=dev";

        var optionsBuilder = new DbContextOptionsBuilder<SolarESDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new SolarESDbContext(optionsBuilder.Options);
    }
}
