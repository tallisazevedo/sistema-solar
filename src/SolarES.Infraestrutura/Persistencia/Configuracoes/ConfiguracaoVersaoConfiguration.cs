using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SolarES.Dominio.Configuracao;
using SolarES.Infraestrutura.Persistencia.Conversoes;

namespace SolarES.Infraestrutura.Persistencia.Configuracoes;

public sealed class ConfiguracaoVersaoConfiguration : IEntityTypeConfiguration<ConfiguracaoVersao>
{
    public void Configure(EntityTypeBuilder<ConfiguracaoVersao> builder)
    {
        builder.Property(c => c.Payload)
            .HasConversion(JsonColuna.CriarConverter<ConfiguracaoCalculo>(), JsonColuna.CriarComparer<ConfiguracaoCalculo>())
            .HasColumnType("jsonb");

        builder.Property(c => c.Observacao).HasMaxLength(2000);

        builder.HasIndex(c => c.Status);
    }
}
