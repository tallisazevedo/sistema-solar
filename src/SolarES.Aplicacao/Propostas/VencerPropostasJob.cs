using Hangfire;
using SolarES.Dominio.Proposta;

namespace SolarES.Aplicacao.Propostas;

public sealed class VencerPropostasJob(
    IPropostaRepository propostas,
    IDadosNotificacaoVencimentoQuery dadosNotificacao,
    INotificadorInterno notificador,
    TimeProvider relogio)
{
    [DisableConcurrentExecution(timeoutInSeconds: 3600)]
    [AutomaticRetry(Attempts = 3)]
    public async Task ExecutarAsync(CancellationToken ct)
    {
        var agora = relogio.GetUtcNow();
        var pendentes = await propostas.ListarPendentesDeVencimentoAsync(agora, ct);

        foreach (var proposta in pendentes)
        {
            if (proposta.Status == StatusProposta.Emitida)
            {
                proposta.Vencer(agora);
            }

            var dados = await dadosNotificacao.ObterAsync(proposta, ct);
            var linhaCliente = dados.Cliente is null ? null : $"Cliente: {dados.Cliente}\n";
            var corpo = $"Proposta: {proposta.Numero}\n{linhaCliente}Data de vencimento: {proposta.ValidaAte:dd/MM/yyyy}";

            await notificador.EnviarEmailAsync(dados.Destinatarios,
                $"Proposta vencida: {proposta.Numero}", corpo, ct);
            proposta.MarcarVencimentoNotificado(agora);
            await propostas.SalvarAlteracoesAsync(ct);
        }
    }
}
