namespace SolarES.Aplicacao.Propostas;

public interface IArmazenamentoPdf
{
    Task<string> SalvarAsync(string numeroProposta, byte[] conteudo, CancellationToken ct);

    Task<byte[]?> LerAsync(string caminho, CancellationToken ct);
}
