namespace SolarES.Dominio.Metricas;

public sealed class EventoFunil : EntidadeBase
{
    private EventoFunil() { }
    public TipoEventoFunil Tipo { get; private set; }
    public Guid SessaoFunilId { get; private set; }
    public DateTimeOffset OcorridoEm { get; private set; }
    public Guid? SimulacaoId { get; private set; }

    public static EventoFunil CriarInicio(Guid sessaoFunilId, DateTimeOffset ocorridoEm) =>
        Criar(TipoEventoFunil.SimulacaoIniciada, sessaoFunilId, ocorridoEm, null);

    public static EventoFunil CriarConclusao(Guid sessaoFunilId, Guid simulacaoId, DateTimeOffset ocorridoEm) =>
        Criar(TipoEventoFunil.SimulacaoConcluida, sessaoFunilId, ocorridoEm, simulacaoId);

    private static EventoFunil Criar(TipoEventoFunil tipo, Guid sessaoFunilId, DateTimeOffset ocorridoEm,
        Guid? simulacaoId)
    {
        if (sessaoFunilId == Guid.Empty) throw new ArgumentException("A sessão do funil é obrigatória.");
        if (tipo == TipoEventoFunil.SimulacaoConcluida && (!simulacaoId.HasValue || simulacaoId == Guid.Empty))
            throw new ArgumentException("A simulação é obrigatória para concluir o funil.");
        return new()
        {
            Id = Guid.NewGuid(),
            Tipo = tipo,
            SessaoFunilId = sessaoFunilId,
            OcorridoEm = ocorridoEm,
            SimulacaoId = simulacaoId,
            CriadoEm = ocorridoEm,
            AtualizadoEm = ocorridoEm,
        };
    }
}
