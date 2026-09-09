namespace SolarES.Dominio.Identidade;

public sealed class Usuario : EntidadeBase
{
    public required string Nome { get; set; }
    public required string Email { get; set; }
    public required string SenhaHash { get; set; }
    public PerfilUsuario Perfil { get; set; }
    public bool Ativo { get; set; }
}
