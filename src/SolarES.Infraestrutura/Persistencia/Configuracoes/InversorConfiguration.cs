using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SolarES.Dominio.Catalogo;

namespace SolarES.Infraestrutura.Persistencia.Configuracoes;

public sealed class InversorConfiguration : IEntityTypeConfiguration<Inversor>
{
    public void Configure(EntityTypeBuilder<Inversor> builder)
    {
        builder.Property(i => i.Fabricante).HasMaxLength(200);
        builder.Property(i => i.Modelo).HasMaxLength(200);
    }
}
