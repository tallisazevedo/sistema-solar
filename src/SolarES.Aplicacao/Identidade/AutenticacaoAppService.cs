using Microsoft.AspNetCore.Identity;
using SolarES.Dominio.Identidade;

namespace SolarES.Aplicacao.Identidade;

public sealed class AutenticacaoAppService(IUsuarioRepository repositorio, IGeradorTokenJwt geradorToken)
{
    private static readonly PasswordHasher<Usuario> Hasher = new();

    public async Task<ResultadoLogin> LoginAsync(string email, string senha, CancellationToken ct)
    {
        var usuario = await repositorio.ObterPorEmailAsync(email, ct);
        if (usuario is null || !usuario.Ativo)
        {
            return ResultadoLogin.Falha();
        }

        var verificacao = Hasher.VerifyHashedPassword(usuario, usuario.SenhaHash, senha);
        if (verificacao == PasswordVerificationResult.Failed)
        {
            return ResultadoLogin.Falha();
        }

        var token = geradorToken.Gerar(usuario);
        return ResultadoLogin.ComSucesso(token, usuario);
    }
}
