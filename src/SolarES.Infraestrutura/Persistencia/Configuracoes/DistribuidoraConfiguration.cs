using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SolarES.Dominio.Tarifas;

namespace SolarES.Infraestrutura.Persistencia.Configuracoes;

public sealed class DistribuidoraConfiguration : IEntityTypeConfiguration<Distribuidora>
{
    public void Configure(EntityTypeBuilder<Distribuidora> builder)
    {
        builder.Property(d => d.Nome).HasMaxLength(200);
        builder.Property(d => d.SiglaAneel).HasMaxLength(20);
    }
}
