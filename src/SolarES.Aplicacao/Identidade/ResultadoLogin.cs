using SolarES.Dominio.Identidade;

namespace SolarES.Aplicacao.Identidade;

public sealed record ResultadoLogin(bool Sucesso, string? Token, Usuario? Usuario)
{
    public static ResultadoLogin Falha() => new(false, null, null);

    public static ResultadoLogin ComSucesso(string token, Usuario usuario) => new(true, token, usuario);
}
