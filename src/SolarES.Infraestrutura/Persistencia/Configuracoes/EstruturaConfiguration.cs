using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SolarES.Dominio.Catalogo;

namespace SolarES.Infraestrutura.Persistencia.Configuracoes;

public sealed class EstruturaConfiguration : IEntityTypeConfiguration<Estrutura>
{
    public void Configure(EntityTypeBuilder<Estrutura> builder)
    {
        builder.Property(e => e.Descricao).HasMaxLength(500);
    }
}
