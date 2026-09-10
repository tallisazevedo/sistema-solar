using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SolarES.Aplicacao.Identidade;
using SolarES.Dominio.Identidade;

namespace SolarES.Infraestrutura.Identidade;

public sealed class GeradorTokenJwt(IConfiguration configuracao) : IGeradorTokenJwt
{
    public string Gerar(Usuario usuario)
    {
        var segredo = configuracao["Jwt:Segredo"]
            ?? throw new InvalidOperationException("Configuracao 'Jwt:Segredo' ausente.");
        var emissor = configuracao["Jwt:Emissor"];
        var audiencia = configuracao["Jwt:Audiencia"];
        var expiraEmMinutos = configuracao.GetValue<int?>("Jwt:ExpiraEmMinutos") ?? 60;

        var credenciais = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(segredo)), SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new Claim(ClaimTypes.Email, usuario.Email),
            new Claim(ClaimTypes.Name, usuario.Nome),
            new Claim(ClaimTypes.Role, usuario.Perfil.ToString()),
        };

        var token = new JwtSecurityToken(
            issuer: emissor,
            audience: audiencia,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiraEmMinutos),
            signingCredentials: credenciais);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
