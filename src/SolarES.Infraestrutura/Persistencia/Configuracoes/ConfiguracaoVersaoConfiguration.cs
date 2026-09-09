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

        builder.HasIndex(c => c.Status, "IX_ConfiguracoesVersao_Status");

        // Rede de seguranca no banco para "no maximo um rascunho / uma publicada"
        // (docs/04) — a Aplicacao ja checa isso antes de gravar, mas o indice garante
        // mesmo sob concorrencia. Precisa do nome já na chamada de HasIndex: como as
        // tres configuracoes miram a mesma unica propriedade (Status), sem o nome
        // explicito aqui o EF Core funde as tres num so indice em vez de criar tres.
        builder.HasIndex(c => c.Status, "IX_ConfiguracoesVersao_UmRascunho")
            .IsUnique()
            .HasFilter($"\"Status\" = {(int)StatusConfiguracaoVersao.Rascunho}");

        builder.HasIndex(c => c.Status, "IX_ConfiguracoesVersao_UmaPublicada")
            .IsUnique()
            .HasFilter($"\"Status\" = {(int)StatusConfiguracaoVersao.Publicada}");
    }
}
