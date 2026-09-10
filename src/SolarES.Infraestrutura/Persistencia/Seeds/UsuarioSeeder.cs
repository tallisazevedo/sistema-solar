using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SolarES.Dominio.Identidade;

namespace SolarES.Infraestrutura.Persistencia.Seeds;

/// <summary>
/// Cria o usuario Dono inicial, exigido para o primeiro login em um banco novo (nao ha
/// endpoint anonimo de cadastro -- violaria "admin rejeita anonimo", T16). Idempotente:
/// so roda se a tabela Usuarios estiver vazia. Credenciais de desenvolvimento devem ser
/// trocadas apos o primeiro login; em producao vem de variavel de ambiente, nunca commitada.
/// </summary>
public static class UsuarioSeeder
{
    private static readonly PasswordHasher<Usuario> Hasher = new();

    public static async Task SeedAsync(SolarESDbContext contexto, string donoEmail, string donoSenha, CancellationToken ct)
    {
        if (await contexto.Usuarios.AnyAsync(ct))
        {
            return;
        }

        var dono = new Usuario
        {
            Id = Guid.NewGuid(),
            Nome = "Dono",
            Email = donoEmail,
            SenhaHash = string.Empty,
            Perfil = PerfilUsuario.Dono,
            Ativo = true,
        };
        dono.SenhaHash = Hasher.HashPassword(dono, donoSenha);

        contexto.Usuarios.Add(dono);
        await contexto.SaveChangesAsync(ct);
    }
}
