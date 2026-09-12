using Microsoft.EntityFrameworkCore;
using SolarES.Aplicacao.Propostas;
using SolarES.Dominio.Identidade;
using PropostaEntidade = SolarES.Dominio.Proposta.Proposta;

namespace SolarES.Infraestrutura.Persistencia.Repositorios;

public sealed class EfDadosNotificacaoVencimentoQuery(SolarESDbContext contexto)
    : IDadosNotificacaoVencimentoQuery
{
    public async Task<DadosNotificacaoVencimento> ObterAsync(PropostaEntidade proposta, CancellationToken ct)
    {
        string[] destinatarios;
        if (proposta.ResponsavelUsuarioId is { } responsavelUsuarioId)
        {
            var email = await contexto.Usuarios
                .Where(u => u.Id == responsavelUsuarioId)
                .Select(u => u.Email)
                .SingleOrDefaultAsync(ct)
                ?? throw new InvalidOperationException($"Responsavel '{responsavelUsuarioId}' nao encontrado.");
            destinatarios = [email];
        }
        else
        {
            destinatarios = await contexto.Usuarios
                .Where(u => u.Ativo && (u.Perfil == PerfilUsuario.Vendedor || u.Perfil == PerfilUsuario.Dono))
                .OrderBy(u => u.Email)
                .Select(u => u.Email)
                .ToArrayAsync(ct);
            if (destinatarios.Length == 0)
                throw new InvalidOperationException("Nenhum vendedor ou dono ativo encontrado para notificar.");
        }

        var cliente = await contexto.Simulacoes
            .Where(s => s.Id == proposta.SimulacaoId && s.LeadId != null)
            .Join(contexto.Leads, s => s.LeadId, l => (Guid?)l.Id, (_, l) => l.Nome)
            .SingleOrDefaultAsync(ct);

        return new(destinatarios, cliente);
    }
}
