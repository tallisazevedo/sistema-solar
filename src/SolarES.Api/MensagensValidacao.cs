using System.Text.RegularExpressions;

namespace SolarES.Api;

/// <summary>
/// As mensagens padrao de ModelState do ASP.NET Core vem em ingles ("The field X is
/// required."). Aqui elas sao traduzidas para um portugues legivel, sem citar o nome
/// tecnico da propriedade (camelCase/PascalCase) na frase.
/// </summary>
public static class MensagensValidacao
{
    public static string Traduzir(string mensagemOriginal)
    {
        if (Regex.IsMatch(mensagemOriginal, @"field is required\.?$", RegexOptions.IgnoreCase))
        {
            return "Campo obrigatorio.";
        }

        var faixa = Regex.Match(mensagemOriginal, @"must be between (.+) and (.+)\.$", RegexOptions.IgnoreCase);
        if (faixa.Success)
        {
            return $"Deve estar entre {faixa.Groups[1].Value} e {faixa.Groups[2].Value}.";
        }

        var tamanhoMaximo = Regex.Match(mensagemOriginal, @"maximum length of (\d+)", RegexOptions.IgnoreCase);
        if (tamanhoMaximo.Success)
        {
            return $"Deve ter no maximo {tamanhoMaximo.Groups[1].Value} caracteres.";
        }

        var tamanhoMinimo = Regex.Match(mensagemOriginal, @"minimum length of (\d+)", RegexOptions.IgnoreCase);
        if (tamanhoMinimo.Success)
        {
            return $"Deve ter no minimo {tamanhoMinimo.Groups[1].Value} caracteres.";
        }

        return "Valor invalido.";
    }

    /// <summary>Transforma uma chave de ModelState ("VisitaTecnicaAgendadaPara") num rotulo legivel.</summary>
    public static string Humanizar(string chaveModelState)
    {
        var ultimoSegmento = chaveModelState.Contains('.')
            ? chaveModelState[(chaveModelState.LastIndexOf('.') + 1)..]
            : chaveModelState;

        if (ultimoSegmento.Length == 0)
        {
            return chaveModelState;
        }

        var comEspacos = Regex.Replace(ultimoSegmento, "(?<=[a-z0-9])(?=[A-Z])", " ");
        return char.ToUpperInvariant(comEspacos[0]) + comEspacos[1..].ToLowerInvariant();
    }
}
