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
        Assert.Throws<ArgumentException>(() => AnexoConta.Criar(
            Guid.NewGuid(), TipoAnexoConta.Pdf, 0, "conta.pdf", DateTimeOffset.UtcNow));
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
}
