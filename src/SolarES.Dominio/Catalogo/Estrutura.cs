using SolarES.Dominio.Simulacao;

namespace SolarES.Dominio.Catalogo;

public sealed class Estrutura : EntidadeBase
{
    public required string Descricao { get; set; }
    public TipoTelhado TipoTelhado { get; set; }
    public bool ResistenteNevoaSalina { get; set; }
    public bool Ativo { get; set; }
}
