using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SolarES.Infraestrutura.Persistencia;

namespace SolarES.Api.Tests;

/// <summary>Troca o SolarESDbContext (Npgsql) por EF InMemory -- os testes de API nao precisam de Postgres rodando.</summary>
public sealed class SolarESApiFactory : WebApplicationFactory<Program>
{
    private readonly string _nomeBase = Guid.NewGuid().ToString();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development"); // MapOpenApi() so registra em Development
        builder.ConfigureServices(services =>
        {
            // AddDbContext registra tanto DbContextOptions<T> quanto IDbContextOptionsConfiguration<T>;
            // remover só o primeiro deixa a configuracao Npgsql do Program.cs "presa" e o EF Core
            // reclama de dois provedores registrados ao mesmo tempo.
            services.RemoveAll<DbContextOptions<SolarESDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<SolarESDbContext>>();
            services.AddDbContext<SolarESDbContext>(options => options.UseInMemoryDatabase(_nomeBase));
        });
    }
}
