using System.ComponentModel.DataAnnotations;
using SolarES.Dominio.Catalogo;
using SolarES.Dominio.Simulacao;

namespace SolarES.Api.Contratos;

public sealed record EstruturaRequest(
    [Required, MaxLength(500)] string Descricao,
    TipoTelhado TipoTelhado,
    bool ResistenteNevoaSalina,
    bool Ativo)
{
    public Estrutura ParaEntidade(Guid id) => new()
    {
        Id = id,
        Descricao = Descricao,
        TipoTelhado = TipoTelhado,
        ResistenteNevoaSalina = ResistenteNevoaSalina,
        Ativo = Ativo,
    };
}

public sealed record EstruturaResponse(
    Guid Id,
    string Descricao,
    TipoTelhado TipoTelhado,
    bool ResistenteNevoaSalina,
    bool Ativo)
{
    public static EstruturaResponse DeEntidade(Estrutura entidade) => new(
        entidade.Id, entidade.Descricao, entidade.TipoTelhado, entidade.ResistenteNevoaSalina, entidade.Ativo);
}
