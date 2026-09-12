using SolarES.Dominio.Lead;

namespace SolarES.Aplicacao.Leads;

public sealed class GerenciarLeadsAppService(ILeadRepository leads,
    ConfiguracaoConsentimentos configuracaoConsentimentos, TimeProvider relogio)
{
    public async Task<Lead> CriarManualAsync(string nome, string telefone, string email, OrigemLead origem,
        bool consentimentoContato, string versaoTexto, CancellationToken ct)
    {
        if (!consentimentoContato) throw new ArgumentException("O consentimento para contato comercial e obrigatorio.");
        if (!configuracaoConsentimentos.VersoesTextoAceitas.Contains(versaoTexto))
            throw new ArgumentException("Versão do texto de consentimento desconhecida.");
        var agora = relogio.GetUtcNow();
        var lead = Lead.CriarManual(nome, telefone, email, origem, agora);
        leads.Adicionar(lead);
        leads.AdicionarConsentimento(ConsentimentoLgpd.Criar(lead.Id,
            FinalidadeConsentimento.ContatoComercial, versaoTexto, agora));
        await leads.SalvarAlteracoesAsync(ct);
        return lead;
    }

    public async Task<bool> AlterarStatusAsync(Guid id, StatusLead status, DateTimeOffset? visitaPara,
        Guid usuarioId, CancellationToken ct)
    {
        var lead = await leads.ObterPorIdAsync(id, ct);
        if (lead is null) return false;
        leads.AdicionarHistorico(lead.AlterarStatus(status, visitaPara, usuarioId, relogio.GetUtcNow()));
        await leads.SalvarAlteracoesAsync(ct);
        return true;
    }
}
