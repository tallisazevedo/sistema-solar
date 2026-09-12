using Microsoft.EntityFrameworkCore;
using SolarES.Aplicacao.Propostas;
using SolarES.Dominio.Proposta;

namespace SolarES.Infraestrutura.Persistencia.Repositorios;

public sealed class EfEnvioPropostaRepository(SolarESDbContext contexto) : IEnvioPropostaRepository
{
    public Task<EnvioProposta?> ObterPorIdAsync(Guid id, CancellationToken ct) =>
        contexto.EnviosProposta.SingleOrDefaultAsync(e => e.Id == id, ct);

    public Task<EnvioProposta?> ObterPorIdMensagemProvedorAsync(string idMensagemProvedor, CancellationToken ct) =>
        contexto.EnviosProposta.SingleOrDefaultAsync(e => e.IdMensagemProvedor == idMensagemProvedor, ct);

    public async Task<IReadOnlyList<EnvioProposta>> ListarPorPropostaAsync(Guid propostaId, CancellationToken ct) =>
        await contexto.EnviosProposta.Where(e => e.PropostaId == propostaId)
            .OrderByDescending(e => e.CriadoEm).ToListAsync(ct);

    public void Adicionar(EnvioProposta envio) => contexto.EnviosProposta.Add(envio);

    public Task<int> ContarEnviadosPorDestinoDesdeAsync(string destinoNormalizado, DateTimeOffset desde, CancellationToken ct) =>
        contexto.EnviosProposta.CountAsync(e =>
            e.EnviadoEm != null && e.EnviadoEm >= desde
            && string.Equals(e.Destino, destinoNormalizado, StringComparison.OrdinalIgnoreCase), ct);

    public Task SalvarAlteracoesAsync(CancellationToken ct) => contexto.SaveChangesAsync(ct);
}
