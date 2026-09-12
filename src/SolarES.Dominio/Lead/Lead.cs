namespace SolarES.Dominio.Lead;

public sealed class Lead : EntidadeBase
{
    private Lead() { }

    public string Nome { get; private set; } = null!;
    public string Telefone { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public OrigemLead Origem { get; private set; }
    public Guid? MunicipioId { get; private set; }
    public Guid? SimulacaoId { get; private set; }
    public CanalPreferido? CanalPreferido { get; private set; }
    public DateTimeOffset? ConsentimentoLgpdEm { get; private set; }
    public StatusLead Status { get; private set; }
    public DateTimeOffset? VisitaTecnicaAgendadaPara { get; private set; }

    public static Lead Criar(string nome, string telefone, string email, CanalPreferido canalPreferido,
        Guid simulacaoId, Guid municipioId, IReadOnlyCollection<FinalidadeConsentimento> finalidades,
        DateTimeOffset momento)
    {
        if (!finalidades.Contains(FinalidadeConsentimento.ContatoComercial))
            throw new ArgumentException("O consentimento para contato comercial é obrigatório.");
        if (string.IsNullOrWhiteSpace(nome)) throw new ArgumentException("Informe seu nome.");
        if (string.IsNullOrWhiteSpace(telefone)) throw new ArgumentException("Informe seu telefone.");
        if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Informe seu e-mail.");
        return new Lead
        {
            Id = Guid.NewGuid(),
            Nome = nome.Trim(),
            Telefone = telefone.Trim(),
            Email = email.Trim(),
            CanalPreferido = canalPreferido,
            SimulacaoId = simulacaoId,
            MunicipioId = municipioId,
            Origem = OrigemLead.Landing,
            Status = StatusLead.Novo,
            ConsentimentoLgpdEm = momento,
            CriadoEm = momento,
            AtualizadoEm = momento,
        };
    }

    public static Lead CriarManual(string nome, string telefone, string email, OrigemLead origem,
        DateTimeOffset momento)
    {
        if (origem == OrigemLead.Landing) throw new ArgumentException("A origem Landing e exclusiva do fluxo publico.");
        ValidarContato(nome, telefone, email);
        return new Lead
        {
            Id = Guid.NewGuid(),
            Nome = nome.Trim(),
            Telefone = telefone.Trim(),
            Email = email.Trim(),
            Origem = origem,
            Status = StatusLead.Novo,
            ConsentimentoLgpdEm = momento,
            CriadoEm = momento,
            AtualizadoEm = momento,
        };
    }

    public HistoricoStatusLead AlterarStatus(StatusLead novoStatus, DateTimeOffset? visitaTecnicaAgendadaPara,
        Guid usuarioId, DateTimeOffset momento)
    {
        var permitido = Status switch
        {
            StatusLead.Novo => novoStatus is StatusLead.EmAtendimento or StatusLead.Perdido,
            StatusLead.EmAtendimento => novoStatus is StatusLead.VisitaTecnicaAgendada or StatusLead.Perdido,
            StatusLead.VisitaTecnicaAgendada => novoStatus is StatusLead.Convertido or StatusLead.Perdido,
            _ => false,
        };
        if (!permitido) throw new TransicaoInvalidaException($"Transicao de {Status} para {novoStatus} nao permitida.");
        if (novoStatus == StatusLead.VisitaTecnicaAgendada && visitaTecnicaAgendadaPara is null)
            throw new ArgumentException("Informe a data da visita tecnica.");

        var anterior = Status;
        Status = novoStatus;
        VisitaTecnicaAgendadaPara = novoStatus == StatusLead.VisitaTecnicaAgendada
            ? visitaTecnicaAgendadaPara : VisitaTecnicaAgendadaPara;
        AtualizadoEm = momento;
        return HistoricoStatusLead.Criar(Id, anterior, novoStatus, usuarioId, momento);
    }

    private static void ValidarContato(string nome, string telefone, string email)
    {
        if (string.IsNullOrWhiteSpace(nome)) throw new ArgumentException("Informe o nome.");
        if (string.IsNullOrWhiteSpace(telefone)) throw new ArgumentException("Informe o telefone.");
        if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Informe o e-mail.");
    }
}
