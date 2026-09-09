using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SolarES.Dominio.Tarifas;
using SolarES.Infraestrutura.Persistencia.Conversoes;

namespace SolarES.Infraestrutura.Persistencia.Configuracoes;

public sealed class MunicipioHspConfiguration : IEntityTypeConfiguration<MunicipioHsp>
{
    public void Configure(EntityTypeBuilder<MunicipioHsp> builder)
    {
        builder.Property(m => m.CodigoIbge).HasMaxLength(7);
        builder.Property(m => m.Nome).HasMaxLength(200);
        builder.Property(m => m.Latitude).HasPrecision(18, 6);
        builder.Property(m => m.Longitude).HasPrecision(18, 6);
        builder.Property(m => m.DistanciaMarKm).HasPrecision(18, 6);
        builder.Property(m => m.Fonte).HasMaxLength(500);

        builder.Property(m => m.HspPorMes)
            .HasConversion(JsonColuna.CriarConverter<IReadOnlyList<decimal>>(), JsonColuna.CriarComparer<IReadOnlyList<decimal>>())
            .HasColumnType("jsonb");

        builder.HasIndex(m => m.CodigoIbge).IsUnique();
    }
}
