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
            Guid.NewGuid(), Guid.NewGuid(), false, DateTimeOffset.UtcNow));
    }

    [Fact]
    public void Dado_ConsentimentoAceito_Quando_CriaLead_Entao_IniciaComoNovoEDaLanding()
    {
        var lead = LeadEntidade.Criar(
            "Maria", "27999999999", "maria@exemplo.com", CanalPreferido.Whatsapp,
            Guid.NewGuid(), Guid.NewGuid(), true, DateTimeOffset.UtcNow);

        Assert.Equal(StatusLead.Novo, lead.Status);
        Assert.Equal(OrigemLead.Landing, lead.Origem);
        Assert.NotNull(lead.ConsentimentoLgpdEm);
    }
}
