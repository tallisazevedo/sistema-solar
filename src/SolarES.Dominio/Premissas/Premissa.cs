namespace SolarES.Dominio.Premissas;

public sealed record Premissa<T>
{
    public T Valor { get; }
    public OrigemPremissa Origem { get; }
    public string? Justificativa { get; }

    public Premissa(T valor, OrigemPremissa origem, string? justificativa = null)
    {
        if (origem == OrigemPremissa.Provisorio && string.IsNullOrWhiteSpace(justificativa))
        {
            throw new ArgumentException(
                "Premissa de origem Provisorio exige justificativa escrita.",
                nameof(justificativa));
        }

        Valor = valor;
        Origem = origem;
        Justificativa = justificativa;
    }
}
