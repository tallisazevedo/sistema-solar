using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SolarES.Dominio.Lead;

namespace SolarES.Infraestrutura.Persistencia.Configuracoes;

public sealed class ConsentimentoLgpdConfiguration : IEntityTypeConfiguration<ConsentimentoLgpd>
{
    public void Configure(EntityTypeBuilder<ConsentimentoLgpd> builder)
    {
        builder.Property(c => c.Finalidade).HasConversion<string>().HasMaxLength(40);
        builder.Property(c => c.VersaoTexto).HasMaxLength(50);
        builder.HasIndex(c => new { c.LeadId, c.Finalidade }).IsUnique();
        builder.HasOne<Lead>().WithMany().HasForeignKey(c => c.LeadId).OnDelete(DeleteBehavior.Cascade);
    }
}
