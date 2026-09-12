using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SolarES.Dominio.Metricas;

namespace SolarES.Infraestrutura.Persistencia.Configuracoes;

public sealed class EventoFunilConfiguration : IEntityTypeConfiguration<EventoFunil>
{
    public void Configure(EntityTypeBuilder<EventoFunil> builder)
    {
        builder.Property(e => e.Tipo).HasConversion<string>().HasMaxLength(30);
        builder.HasIndex(e => new { e.Tipo, e.OcorridoEm });
    }
}
