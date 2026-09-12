using SolarES.Dominio.Lead;
using LeadEntidade = SolarES.Dominio.Lead.Lead;

namespace SolarES.Dominio.Tests;

public sealed class LeadTests
{
    [Fact]
    public void Dado_ConsentimentoAusente_Quando_CriaLead_Entao_RecusaCriacao()
    {
        Assert.Throws<ArgumentException>(() => LeadEntidade.Criar(
            "Maria", "27999999999", "maria@exemplo.com", CanalPreferido.Email,
            Guid.NewGuid(), Guid.NewGuid(), [], DateTimeOffset.UtcNow));
    }

    [Fact]
    public void Dado_ConsentimentoAceito_Quando_CriaLead_Entao_IniciaComoNovoEDaLanding()
    {
        var lead = LeadEntidade.Criar(
            "Maria", "27999999999", "maria@exemplo.com", CanalPreferido.Whatsapp,
            Guid.NewGuid(), Guid.NewGuid(), [FinalidadeConsentimento.ContatoComercial], DateTimeOffset.UtcNow);

        Assert.Equal(StatusLead.Novo, lead.Status);
        Assert.Equal(OrigemLead.Landing, lead.Origem);
        Assert.NotNull(lead.ConsentimentoLgpdEm);
    }

    [Fact]
    public void Dado_ArquivoVazio_Quando_CriaAnexo_Entao_RecusaCriacao()
    {
        var recebidoEm = DateTimeOffset.UtcNow;
        Assert.Throws<ArgumentException>(() => AnexoConta.Criar(
            Guid.NewGuid(), TipoAnexoConta.Pdf, 0, "conta.pdf", recebidoEm, recebidoEm.AddDays(90)));
    }

    [Fact]
    public void Dado_PrazoDescarteAnteriorAoRecebimento_Quando_CriaAnexo_Entao_RecusaCriacao()
    {
        var recebidoEm = DateTimeOffset.UtcNow;
        Assert.Throws<ArgumentException>(() => AnexoConta.Criar(
            Guid.NewGuid(), TipoAnexoConta.Pdf, 100, "conta.pdf", recebidoEm, recebidoEm.AddDays(-1)));
    }

    [Fact]
    public void Dado_AnexoValido_Quando_Descarta_Entao_PreenchaDescartadoEm()
    {
        var recebidoEm = DateTimeOffset.UtcNow;
        var anexo = AnexoConta.Criar(Guid.NewGuid(), TipoAnexoConta.Pdf, 100, "conta.pdf",
            recebidoEm, recebidoEm.AddDays(90));

        var momentoDescarte = recebidoEm.AddDays(91);
        anexo.Descartar(momentoDescarte);

        Assert.Equal(momentoDescarte, anexo.DescartadoEm);
    }

    [Fact]
    public void Dado_AnexoJaDescartado_Quando_DescartaNovamente_Entao_Recusa()
    {
        var recebidoEm = DateTimeOffset.UtcNow;
        var anexo = AnexoConta.Criar(Guid.NewGuid(), TipoAnexoConta.Pdf, 100, "conta.pdf",
            recebidoEm, recebidoEm.AddDays(90));
        anexo.Descartar(recebidoEm.AddDays(91));

        Assert.Throws<InvalidOperationException>(() => anexo.Descartar(recebidoEm.AddDays(92)));
    }

    [Fact]
    public void Dada_FinalidadeEData_Quando_RegistraConsentimento_Entao_PreservaEvidencia()
    {
        var momento = new DateTimeOffset(2026, 9, 11, 10, 0, 0, TimeSpan.Zero);
        var consentimento = ConsentimentoLgpd.Criar(Guid.NewGuid(),
            FinalidadeConsentimento.ContatoComercial, "contato-comercial-v1", momento);

        Assert.Equal(FinalidadeConsentimento.ContatoComercial, consentimento.Finalidade);
        Assert.Equal("contato-comercial-v1", consentimento.VersaoTexto);
        Assert.Equal(momento, consentimento.ConcedidoEm);
    }

    [Fact]
    public void Dado_LeadNovo_Quando_AvancaAteConvertido_Entao_RegistraTransicoesEVisita()
    {
        var agora = DateTimeOffset.UtcNow;
        var usuarioId = Guid.NewGuid();
        var visita = agora.AddDays(2);
        var lead = LeadEntidade.CriarManual("Maria", "27999999999", "maria@exemplo.com",
            OrigemLead.Indicacao, agora);

        var inicio = lead.AlterarStatus(StatusLead.EmAtendimento, null, usuarioId, agora);
        lead.AlterarStatus(StatusLead.VisitaTecnicaAgendada, visita, usuarioId, agora);
        lead.AlterarStatus(StatusLead.Convertido, null, usuarioId, agora);

        Assert.Equal(StatusLead.Novo, inicio.StatusAnterior);
        Assert.Equal(StatusLead.EmAtendimento, inicio.StatusNovo);
        Assert.Equal(visita, lead.VisitaTecnicaAgendadaPara);
        Assert.Equal(StatusLead.Convertido, lead.Status);
    }

    [Fact]
    public void Dado_LeadConvertido_Quando_TentaNovaTransicao_Entao_Recusa()
    {
        var agora = DateTimeOffset.UtcNow;
        var usuarioId = Guid.NewGuid();
        var lead = LeadEntidade.CriarManual("Maria", "27999999999", "maria@exemplo.com",
            OrigemLead.Telefone, agora);
        lead.AlterarStatus(StatusLead.EmAtendimento, null, usuarioId, agora);
        lead.AlterarStatus(StatusLead.VisitaTecnicaAgendada, agora.AddDays(1), usuarioId, agora);
        lead.AlterarStatus(StatusLead.Convertido, null, usuarioId, agora);

        Assert.Throws<TransicaoInvalidaException>(() =>
            lead.AlterarStatus(StatusLead.Perdido, null, usuarioId, agora));
    }
}
