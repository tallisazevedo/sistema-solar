using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LeadEntidade = SolarES.Dominio.Lead.Lead;

namespace SolarES.Infraestrutura.Persistencia.Configuracoes;

public sealed class LeadConfiguration : IEntityTypeConfiguration<LeadEntidade>
{
    public void Configure(EntityTypeBuilder<LeadEntidade> builder)
    {
        builder.Property(l => l.Nome).HasMaxLength(200);
        builder.Property(l => l.Telefone).HasMaxLength(30);
        builder.Property(l => l.Email).HasMaxLength(300);
        builder.Property(l => l.Origem).HasConversion<string>().HasMaxLength(30);
        builder.Property(l => l.Status).HasConversion<string>().HasMaxLength(30);
        builder.Property(l => l.CanalPreferido).HasConversion<string>().HasMaxLength(20);
        builder.HasIndex(l => l.SimulacaoId).IsUnique();

        builder.HasIndex(l => l.CriadoEm).IsDescending();
        builder.HasIndex(l => l.Status);
    }
}
