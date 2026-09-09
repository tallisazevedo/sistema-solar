using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace SolarES.Infraestrutura.Persistencia.Conversoes;

/// <summary>
/// Serializacao para as colunas jsonb: tipos ricos do Dominio (Premissa&lt;T&gt;,
/// ConfiguracaoCalculo, listas de decimal) viram texto JSON na coluna e voltam a ser
/// o tipo original na leitura. O Dominio nunca sabe disso — a conversao vive só aqui.
/// </summary>
public static class JsonColuna
{
    private static readonly JsonSerializerOptions Opcoes = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() },
    };

    public static ValueConverter<T, string> CriarConverter<T>() => new(
        valor => JsonSerializer.Serialize(valor, Opcoes),
        json => JsonSerializer.Deserialize<T>(json, Opcoes)!);

    public static ValueComparer<T> CriarComparer<T>() => new(
        (a, b) => JsonSerializer.Serialize(a, Opcoes) == JsonSerializer.Serialize(b, Opcoes),
        valor => JsonSerializer.Serialize(valor, Opcoes).GetHashCode(),
        valor => JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(valor, Opcoes), Opcoes)!);
}
