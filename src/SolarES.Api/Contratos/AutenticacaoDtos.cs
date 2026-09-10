using System.ComponentModel.DataAnnotations;
using SolarES.Dominio.Identidade;

namespace SolarES.Api.Contratos;

public sealed record LoginRequest(
    [Required, EmailAddress] string Email,
    [Required] string Senha);

public sealed record LoginResponse(string Token, string Nome, PerfilUsuario Perfil);
