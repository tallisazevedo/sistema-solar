using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SolarES.Aplicacao.Identidade;
using SolarES.Dominio.Identidade;
using SolarES.Infraestrutura.Persistencia;
using SolarES.Infraestrutura.Persistencia.Repositorios;

namespace SolarES.Aplicacao.Tests;

public class AutenticacaoAppServiceTests
{
    private const string SenhaValida = "SenhaValida!123";

    private static (AutenticacaoAppService Servico, SolarESDbContext Contexto) CriarServico()
    {
        var options = new DbContextOptionsBuilder<SolarESDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var contexto = new SolarESDbContext(options);
        var repositorio = new EfUsuarioRepository(contexto);
        var servico = new AutenticacaoAppService(repositorio, new GeradorTokenJwtFalso());

        return (servico, contexto);
    }

    private static Usuario CriarUsuario(PerfilUsuario perfil, bool ativo, string email)
    {
        var usuario = new Usuario { Id = Guid.NewGuid(), Nome = "Teste", Email = email, SenhaHash = string.Empty, Perfil = perfil, Ativo = ativo };
        usuario.SenhaHash = new PasswordHasher<Usuario>().HashPassword(usuario, SenhaValida);
        return usuario;
    }

    [Fact]
    public async Task LoginAsync_ComSenhaCorreta_RetornaSucessoComToken()
    {
        var (servico, contexto) = CriarServico();
        var usuario = CriarUsuario(PerfilUsuario.Dono, ativo: true, email: "dono@teste.com");
        contexto.Usuarios.Add(usuario);
        await contexto.SaveChangesAsync();

        var resultado = await servico.LoginAsync(usuario.Email, SenhaValida, CancellationToken.None);

        Assert.True(resultado.Sucesso);
        Assert.False(string.IsNullOrWhiteSpace(resultado.Token));
    }

    [Fact]
    public async Task LoginAsync_ComSenhaErrada_RetornaFalha()
    {
        var (servico, contexto) = CriarServico();
        var usuario = CriarUsuario(PerfilUsuario.Dono, ativo: true, email: "dono@teste.com");
        contexto.Usuarios.Add(usuario);
        await contexto.SaveChangesAsync();

        var resultado = await servico.LoginAsync(usuario.Email, "senha-errada", CancellationToken.None);

        Assert.False(resultado.Sucesso);
        Assert.Null(resultado.Token);
    }

    [Fact]
    public async Task LoginAsync_ComUsuarioInativo_RetornaFalha()
    {
        var (servico, contexto) = CriarServico();
        var usuario = CriarUsuario(PerfilUsuario.Vendedor, ativo: false, email: "inativo@teste.com");
        contexto.Usuarios.Add(usuario);
        await contexto.SaveChangesAsync();

        var resultado = await servico.LoginAsync(usuario.Email, SenhaValida, CancellationToken.None);

        Assert.False(resultado.Sucesso);
    }

    [Fact]
    public async Task LoginAsync_ComEmailInexistente_RetornaFalha()
    {
        var (servico, _) = CriarServico();

        var resultado = await servico.LoginAsync("ninguem@teste.com", SenhaValida, CancellationToken.None);

        Assert.False(resultado.Sucesso);
    }

    private sealed class GeradorTokenJwtFalso : IGeradorTokenJwt
    {
        public string Gerar(Usuario usuario) => $"token-de-teste-{usuario.Id}";
    }
}
