using Microsoft.EntityFrameworkCore;
using SolarES.Dominio.Catalogo;
using SolarES.Dominio.Configuracao;
using SolarES.Dominio.Conta;
using SolarES.Dominio.Identidade;
using SolarES.Dominio.Precificacao;
using SolarES.Dominio.Tarifas;
using SolarES.Dominio.Lead;
using PropostaEntidade = SolarES.Dominio.Proposta.Proposta;
using SimulacaoEntidade = SolarES.Dominio.Simulacao.Simulacao;
using LeadEntidade = SolarES.Dominio.Lead.Lead;

namespace SolarES.Infraestrutura.Persistencia;

public sealed class SolarESDbContext(DbContextOptions<SolarESDbContext> options) : DbContext(options)
{
    public DbSet<ConfiguracaoVersao> ConfiguracoesVersao => Set<ConfiguracaoVersao>();
    public DbSet<ModuloFotovoltaico> ModulosFotovoltaicos => Set<ModuloFotovoltaico>();
    public DbSet<Inversor> Inversores => Set<Inversor>();
    public DbSet<Estrutura> Estruturas => Set<Estrutura>();
    public DbSet<FaixaPreco> FaixasPreco => Set<FaixaPreco>();
    public DbSet<Distribuidora> Distribuidoras => Set<Distribuidora>();
    public DbSet<TarifaVigente> TarifasVigentes => Set<TarifaVigente>();
    public DbSet<MunicipioHsp> MunicipiosHsp => Set<MunicipioHsp>();
    public DbSet<LeadEntidade> Leads => Set<LeadEntidade>();
    public DbSet<AnexoConta> AnexosConta => Set<AnexoConta>();
    public DbSet<ConsentimentoLgpd> ConsentimentosLgpd => Set<ConsentimentoLgpd>();
    public DbSet<ContaExtraida> ContasExtraidas => Set<ContaExtraida>();
    public DbSet<SimulacaoEntidade> Simulacoes => Set<SimulacaoEntidade>();
    public DbSet<PropostaEntidade> Propostas => Set<PropostaEntidade>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SolarESDbContext).Assembly);
    }
}
