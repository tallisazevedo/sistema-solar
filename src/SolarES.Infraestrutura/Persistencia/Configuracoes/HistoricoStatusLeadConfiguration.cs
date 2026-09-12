using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SolarES.Dominio.Identidade;
using SolarES.Dominio.Lead;

namespace SolarES.Infraestrutura.Persistencia.Configuracoes;

public sealed class HistoricoStatusLeadConfiguration : IEntityTypeConfiguration<HistoricoStatusLead>
{
    public void Configure(EntityTypeBuilder<HistoricoStatusLead> builder)
    {
        builder.Property(h => h.StatusAnterior).HasConversion<string>().HasMaxLength(30);
        builder.Property(h => h.StatusNovo).HasConversion<string>().HasMaxLength(30);
        builder.HasIndex(h => new { h.LeadId, h.AlteradoEm });
        builder.HasOne<Lead>().WithMany().HasForeignKey(h => h.LeadId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<Usuario>().WithMany().HasForeignKey(h => h.UsuarioId).OnDelete(DeleteBehavior.Restrict);
    }
}
