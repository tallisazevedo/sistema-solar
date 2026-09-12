using PropostaEntidade = SolarES.Dominio.Proposta.Proposta;

namespace SolarES.Aplicacao.Propostas;

public sealed record DadosNotificacaoVencimento(IReadOnlyCollection<string> Destinatarios, string? Cliente);

public interface IDadosNotificacaoVencimentoQuery
{
    Task<DadosNotificacaoVencimento> ObterAsync(PropostaEntidade proposta, CancellationToken ct);
}
