using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SolarES.Dominio.Tarifas;

namespace SolarES.Infraestrutura.Persistencia.Configuracoes;

public sealed class TarifaVigenteConfiguration : IEntityTypeConfiguration<TarifaVigente>
{
    public void Configure(EntityTypeBuilder<TarifaVigente> builder)
    {
        builder.Property(t => t.TarifaTe).HasPrecision(18, 6);
        builder.Property(t => t.TarifaTusd).HasPrecision(18, 6);
        builder.Property(t => t.ValorFioBPorKwh).HasPrecision(18, 6);
        builder.Property(t => t.AliquotaIcms).HasPrecision(18, 6);
        builder.Property(t => t.AliquotaPisCofins).HasPrecision(18, 6);
        builder.Property(t => t.ResolucaoHomologatoria).HasMaxLength(200);
        builder.Property(t => t.Fonte).HasMaxLength(2000);

        builder.HasIndex(t => new { t.DistribuidoraId, t.Subgrupo, t.VigenciaInicio });
    }
}
