namespace SolarES.Aplicacao.Propostas;

/// <summary>
/// Issue #33: anti-spam do envio automatico e do disparo do canal -- no maximo
/// LimitePorDestino envios/leads por destino dentro da Janela.
/// </summary>
public sealed record ConfiguracaoLimiteEnvios(int LimitePorDestino, TimeSpan Janela);
