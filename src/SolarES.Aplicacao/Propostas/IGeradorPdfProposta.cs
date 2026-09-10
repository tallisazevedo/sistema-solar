using SolarES.Dominio.Configuracao;
using SolarES.Dominio.Simulacao;

namespace SolarES.Aplicacao.Propostas;

public interface IGeradorPdfProposta
{
    byte[] Gerar(string numero, DateTimeOffset validaAte, ConfiguracaoCalculo configuracao, ResultadoSimulacao resultado);
}
