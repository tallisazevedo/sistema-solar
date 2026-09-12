using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SolarES.Dominio.Proposta;
using PropostaEntidade = SolarES.Dominio.Proposta.Proposta;

namespace SolarES.Infraestrutura.Persistencia.Configuracoes;

public sealed class EnvioPropostaConfiguration : IEntityTypeConfiguration<EnvioProposta>
{
    public void Configure(EntityTypeBuilder<EnvioProposta> builder)
    {
        builder.Property(e => e.Destino).HasMaxLength(300);
        builder.Property(e => e.IdMensagemProvedor).HasMaxLength(300);

        builder.HasIndex(e => e.PropostaId);
        builder.HasOne<PropostaEntidade>().WithMany().HasForeignKey(e => e.PropostaId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
