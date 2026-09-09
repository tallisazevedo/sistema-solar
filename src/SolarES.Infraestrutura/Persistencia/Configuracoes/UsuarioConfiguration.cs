using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SolarES.Dominio.Identidade;

namespace SolarES.Infraestrutura.Persistencia.Configuracoes;

public sealed class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.Property(u => u.Nome).HasMaxLength(200);
        builder.Property(u => u.Email).HasMaxLength(300);
        builder.Property(u => u.SenhaHash).HasMaxLength(500);

        builder.HasIndex(u => u.Email).IsUnique();
    }
}
