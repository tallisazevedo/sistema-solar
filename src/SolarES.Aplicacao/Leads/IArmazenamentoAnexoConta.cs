using SolarES.Dominio.Lead;

namespace SolarES.Aplicacao.Leads;

public interface IArmazenamentoAnexoConta
{
    Task<string> SalvarAsync(Guid leadId, TipoAnexoConta tipo, byte[] conteudo, CancellationToken ct);
    Task<byte[]?> LerAsync(string caminho, CancellationToken ct);
    Task ApagarAsync(string caminho, CancellationToken ct);
}
