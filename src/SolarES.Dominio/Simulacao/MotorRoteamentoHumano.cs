using SolarES.Dominio.Configuracao;

namespace SolarES.Dominio.Simulacao;

/// <summary>
/// Roteamento para humano (docs/02, regra de negocio 4). "Grupo A" nao entra aqui —
/// o tipo do sistema so representa Grupo B (docs/02: "O MVP atende so o Grupo B"),
/// entao nao ha caminho no codigo capaz de criar uma entrada de Grupo A para checar.
/// </summary>
public static class MotorRoteamentoHumano
{
    public static (bool Roteada, string? Motivo) Avaliar(
        EntradaSimulacao entrada,
        ConfiguracaoCalculo configuracao,
        decimal potenciaInstaladaKwp)
    {
        if (entrada.PossuiGeracaoPropria)
        {
            return (true, "Conta ja possui geracao propria instalada.");
        }

        var limite = configuracao.LimiteKwpRoteamentoHumano.Valor;
        if (potenciaInstaladaKwp > limite)
        {
            return (true, $"Potencia dimensionada ({potenciaInstaladaKwp:F2} kWp) acima do limite configurado para fluxo automatico ({limite:F2} kWp).");
        }

        return (false, null);
    }
}
