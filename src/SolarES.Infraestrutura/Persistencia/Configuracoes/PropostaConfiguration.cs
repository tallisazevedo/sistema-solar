using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PropostaEntidade = SolarES.Dominio.Proposta.Proposta;

namespace SolarES.Infraestrutura.Persistencia.Configuracoes;

public sealed class PropostaConfiguration : IEntityTypeConfiguration<PropostaEntidade>
{
    public void Configure(EntityTypeBuilder<PropostaEntidade> builder)
    {
        builder.Property(p => p.Numero).HasMaxLength(50);
        builder.Property(p => p.ArquivoPdfUrl).HasMaxLength(1000);

        builder.HasIndex(p => p.Numero).IsUnique();
        builder.HasIndex(p => p.ValidaAte);
    }
}
