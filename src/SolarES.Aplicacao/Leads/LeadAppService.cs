using System.Text.Json;
using SolarES.Aplicacao.Configuracao;
using SolarES.Aplicacao.Propostas;
using SolarES.Aplicacao.Simulacoes;
using SolarES.Dominio.Configuracao;
using SolarES.Dominio.Lead;
using SolarES.Dominio.Simulacao;

namespace SolarES.Aplicacao.Leads;

public sealed class LeadAppService(ILeadRepository leads, ISimulacaoRepository simulacoes,
    IConfiguracaoVersaoRepository configuracoes, PropostaAppService propostas, TimeProvider relogio)
{
    public async Task<DesfechoCapturaLead> CapturarPublicoAsync(Guid simulacaoId, string nome,
        string telefone, string email, CanalPreferido canal, bool consentimento, CancellationToken ct)
    {
        var simulacao = await simulacoes.ObterPorIdAsync(simulacaoId, ct);
        if (simulacao is null || simulacao.Origem != OrigemSimulacao.Landing)
            throw new KeyNotFoundException("Simulação pública não encontrada.");
        var entrada = JsonSerializer.Deserialize<EntradaSimulacao>(simulacao.EntradasSnapshot)
            ?? throw new InvalidOperationException("Entradas da simulação inválidas.");
        var municipioId = await leads.ObterMunicipioIdPorCodigoAsync(entrada.MunicipioCodigoIbge, ct)
            ?? throw new InvalidOperationException("Município da simulação não encontrado.");
        var lead = Lead.Criar(nome, telefone, email, canal, simulacao.Id, municipioId,
            consentimento, relogio.GetUtcNow());
        leads.Adicionar(lead);
        simulacao.LeadId = lead.Id;
        await leads.SalvarAlteracoesAsync(ct);

        var versao = await configuracoes.ObterPorIdAsync(simulacao.ConfiguracaoVersaoId, ct)
            ?? throw new InvalidOperationException("Versão da configuração não encontrada.");
        if (simulacao.RoteadaParaHumano) return DesfechoCapturaLead.RoteadoParaHumano;
        if (versao.Payload.PossuiPremissaProvisoria()) return DesfechoCapturaLead.CalibracaoPendente;
        await propostas.GerarAsync(simulacao.Id, ct);
        return DesfechoCapturaLead.PropostaEmitida;
    }
}

public enum DesfechoCapturaLead { CalibracaoPendente, RoteadoParaHumano, PropostaEmitida }
