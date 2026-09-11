namespace SolarES.Dominio.Lead;

public sealed class Lead : EntidadeBase
{
    private Lead() { }

    public string Nome { get; private set; } = null!;
    public string Telefone { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public OrigemLead Origem { get; private set; }
    public Guid MunicipioId { get; private set; }
    public Guid? SimulacaoId { get; private set; }
    public CanalPreferido? CanalPreferido { get; private set; }
    public DateTimeOffset? ConsentimentoLgpdEm { get; private set; }
    public StatusLead Status { get; private set; }

    public static Lead Criar(string nome, string telefone, string email, CanalPreferido canalPreferido,
        Guid simulacaoId, Guid municipioId, bool consentimento, DateTimeOffset momento)
    {
        if (!consentimento) throw new ArgumentException("O consentimento para contato é obrigatório.");
        if (string.IsNullOrWhiteSpace(nome)) throw new ArgumentException("Informe seu nome.");
        if (string.IsNullOrWhiteSpace(telefone)) throw new ArgumentException("Informe seu telefone.");
        if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Informe seu e-mail.");
        return new Lead
        {
            Id = Guid.NewGuid(), Nome = nome.Trim(), Telefone = telefone.Trim(), Email = email.Trim(),
            CanalPreferido = canalPreferido, SimulacaoId = simulacaoId, MunicipioId = municipioId,
            Origem = OrigemLead.Landing, Status = StatusLead.Novo, ConsentimentoLgpdEm = momento,
            CriadoEm = momento, AtualizadoEm = momento,
        };
    }
}
