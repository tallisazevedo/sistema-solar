using Microsoft.EntityFrameworkCore;
using SolarES.Aplicacao.Propostas;
using PropostaEntidade = SolarES.Dominio.Proposta.Proposta;

namespace SolarES.Infraestrutura.Persistencia.Repositorios;

public sealed class EfPropostaRepository(SolarESDbContext contexto) : IPropostaRepository
{
    public Task<int> ContarAsync(CancellationToken ct) => contexto.Propostas.CountAsync(ct);

    public Task<PropostaEntidade?> ObterPorIdAsync(Guid id, CancellationToken ct) =>
        contexto.Propostas.SingleOrDefaultAsync(p => p.Id == id, ct);

    public void Adicionar(PropostaEntidade proposta) => contexto.Propostas.Add(proposta);

    public Task SalvarAlteracoesAsync(CancellationToken ct) => contexto.SaveChangesAsync(ct);
}
