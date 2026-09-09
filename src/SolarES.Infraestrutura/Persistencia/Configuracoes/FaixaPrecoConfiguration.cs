using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SolarES.Dominio.Precificacao;

namespace SolarES.Infraestrutura.Persistencia.Configuracoes;

public sealed class FaixaPrecoConfiguration : IEntityTypeConfiguration<FaixaPreco>
{
    public void Configure(EntityTypeBuilder<FaixaPreco> builder)
    {
        builder.Property(f => f.KwpMinimo).HasPrecision(18, 6);
        builder.Property(f => f.KwpMaximo).HasPrecision(18, 6);
        builder.Property(f => f.PrecoPorWp).HasPrecision(18, 6);
        builder.Property(f => f.TipoInstalacao).HasMaxLength(100);
    }
}
