using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SolarES.Dominio.Catalogo;

namespace SolarES.Infraestrutura.Persistencia.Configuracoes;

public sealed class ModuloFotovoltaicoConfiguration : IEntityTypeConfiguration<ModuloFotovoltaico>
{
    public void Configure(EntityTypeBuilder<ModuloFotovoltaico> builder)
    {
        builder.Property(m => m.Fabricante).HasMaxLength(200);
        builder.Property(m => m.Modelo).HasMaxLength(200);
        builder.Property(m => m.EficienciaPercentual).HasPrecision(18, 6);
    }
}
