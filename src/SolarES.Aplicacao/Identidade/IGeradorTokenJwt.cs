using SolarES.Dominio.Identidade;

namespace SolarES.Aplicacao.Identidade;

public interface IGeradorTokenJwt
{
    string Gerar(Usuario usuario);
}
