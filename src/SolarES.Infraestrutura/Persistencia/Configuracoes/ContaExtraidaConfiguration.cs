using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SolarES.Dominio.Conta;
using SolarES.Infraestrutura.Persistencia.Conversoes;

namespace SolarES.Infraestrutura.Persistencia.Configuracoes;

public sealed class ContaExtraidaConfiguration : IEntityTypeConfiguration<ContaExtraida>
{
    public void Configure(EntityTypeBuilder<ContaExtraida> builder)
    {
        builder.Property(c => c.NumeroUc).HasMaxLength(50);
        builder.Property(c => c.TarifaExtraida).HasPrecision(18, 6);

        builder.Property(c => c.HistoricoConsumo)
            .HasConversion(JsonColuna.CriarConverter<IReadOnlyList<decimal>>(), JsonColuna.CriarComparer<IReadOnlyList<decimal>>())
            .HasColumnType("jsonb");

        builder.Property(c => c.PayloadBruto).HasColumnType("jsonb");
    }
}
