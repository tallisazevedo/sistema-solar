using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SolarES.Dominio.Lead;

namespace SolarES.Infraestrutura.Persistencia.Configuracoes;

public sealed class AnexoContaConfiguration : IEntityTypeConfiguration<AnexoConta>
{
    public void Configure(EntityTypeBuilder<AnexoConta> builder)
    {
        builder.Property(a => a.Tipo).HasConversion<string>().HasMaxLength(10);
        builder.Property(a => a.CaminhoArmazenamento).HasMaxLength(1000);
        builder.HasIndex(a => a.LeadId).IsUnique();
        builder.HasIndex(a => a.DescartarAte);
        builder.HasOne<Lead>().WithOne().HasForeignKey<AnexoConta>(a => a.LeadId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
