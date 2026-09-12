using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SimulacaoEntidade = SolarES.Dominio.Simulacao.Simulacao;

namespace SolarES.Infraestrutura.Persistencia.Configuracoes;

public sealed class SimulacaoConfiguration : IEntityTypeConfiguration<SimulacaoEntidade>
{
    public void Configure(EntityTypeBuilder<SimulacaoEntidade> builder)
    {
        builder.Property(s => s.EntradasSnapshot).HasColumnType("jsonb");
        builder.Property(s => s.ResultadoSnapshot).HasColumnType("jsonb");
        builder.Property(s => s.Origem).HasConversion<string>().HasMaxLength(20);

        builder.Property(s => s.PotenciaKwp).HasPrecision(18, 6);
        builder.Property(s => s.Capex).HasPrecision(18, 2);
        builder.Property(s => s.EconomiaMensalAno1).HasPrecision(18, 2);
        builder.Property(s => s.Tir).HasPrecision(18, 6);
        builder.Property(s => s.Vpl).HasPrecision(18, 2);
        builder.Property(s => s.CoberturaPercentual).HasPrecision(18, 6);
        builder.Property(s => s.MotivoRoteamento).HasMaxLength(500);

        builder.HasIndex(s => s.LeadId);
        builder.HasIndex(s => s.ConfiguracaoVersaoId);
    }
}
