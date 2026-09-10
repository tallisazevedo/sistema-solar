using System.ComponentModel.DataAnnotations;
using SolarES.Dominio.Configuracao;

namespace SolarES.Api.Contratos;

public sealed record CriarRascunhoRequest(
    [Required] ConfiguracaoCalculo Payload,
    string? Observacao);

public sealed record ConfiguracaoCalculoRequest([Required] ConfiguracaoCalculo Payload);

public sealed record PublicarRascunhoRequest([Required] Guid UsuarioId);

public sealed record ConfiguracaoVersaoResponse(
    Guid Id,
    int Numero,
    StatusConfiguracaoVersao Status,
    ConfiguracaoCalculo Payload,
    DateTimeOffset? PublicadaEm,
    Guid? PublicadaPorUsuarioId,
    string? Observacao)
{
    public static ConfiguracaoVersaoResponse DeEntidade(ConfiguracaoVersao entidade) => new(
        entidade.Id, entidade.Numero, entidade.Status, entidade.Payload,
        entidade.PublicadaEm, entidade.PublicadaPorUsuarioId, entidade.Observacao);
}
