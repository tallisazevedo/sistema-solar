using SolarES.Dominio.Lead;

namespace SolarES.Aplicacao.Leads;

/// <summary>
/// Anonimizacao compartilhada entre o expurgo automatico (T24) e a eliminacao a pedido
/// do titular: mesmo efeito, dois gatilhos diferentes.
/// </summary>
internal static class AnonimizacaoLead
{
    public static async Task ExecutarAsync(Lead lead, AnexoConta? anexo,
        IArmazenamentoAnexoConta armazenamentoAnexos, DateTimeOffset momento, CancellationToken ct)
    {
        if (anexo is not null && anexo.DescartadoEm is null)
        {
            await armazenamentoAnexos.ApagarAsync(anexo.CaminhoArmazenamento, ct);
            anexo.Descartar(momento);
        }
        lead.Expurgar(momento);
    }
}
