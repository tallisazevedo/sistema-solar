using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using SolarES.Dominio.Identidade;
using SolarES.Infraestrutura.Persistencia;

namespace SolarES.Api.Tests;

/// <summary>Troca o SolarESDbContext (Npgsql) por EF InMemory -- os testes de API nao precisam de Postgres rodando.</summary>
public sealed class SolarESApiFactory : WebApplicationFactory<Program>
{
    public const string DonoEmail = "dono@teste.solares";
    public const string DonoSenha = "SenhaDono!123";
    public const string VendedorEmail = "vendedor@teste.solares";
    public const string VendedorSenha = "SenhaVendedor!123";

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

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);

        using var escopo = host.Services.CreateScope();
        var contexto = escopo.ServiceProvider.GetRequiredService<SolarESDbContext>();
        var hasher = new PasswordHasher<Usuario>();

        var dono = new Usuario { Id = Guid.NewGuid(), Nome = "Dono Teste", Email = DonoEmail, SenhaHash = string.Empty, Perfil = PerfilUsuario.Dono, Ativo = true };
        dono.SenhaHash = hasher.HashPassword(dono, DonoSenha);

        var vendedor = new Usuario { Id = Guid.NewGuid(), Nome = "Vendedor Teste", Email = VendedorEmail, SenhaHash = string.Empty, Perfil = PerfilUsuario.Vendedor, Ativo = true };
        vendedor.SenhaHash = hasher.HashPassword(vendedor, VendedorSenha);

        contexto.Usuarios.AddRange(dono, vendedor);
        contexto.SaveChanges();

        return host;
    }

    public static async Task<HttpClient> ClienteAutenticadoAsync(HttpClient cliente, string email, string senha)
    {
        var response = await cliente.PostAsJsonAsync("/api/auth/login", new { Email = email, Senha = senha });
        response.EnsureSuccessStatusCode();
        var corpo = await response.Content.ReadFromJsonAsync<LoginResponseTeste>();

        cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", corpo!.Token);
        return cliente;
    }

    private sealed record LoginResponseTeste(string Token, string Nome, PerfilUsuario Perfil);
}
