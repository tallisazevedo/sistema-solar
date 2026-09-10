using System.Reflection;
using SolarES.Dominio.Premissas;

namespace SolarES.Dominio.Configuracao;

public static class ConfiguracaoCalculoExtensoes
{
    /// <summary>
    /// Verdadeiro se qualquer premissa de <paramref name="configuracao"/> tiver
    /// Origem Provisorio -- decide a marca d'agua "calibracao pendente" do PDF (T20).
    /// Usa reflexao sobre as propriedades publicas em vez de checar campo a campo pra
    /// nunca ficar desatualizado: o teste de invariante da T03
    /// (TodaPropriedade_DeConfiguracaoCalculo_EhPremissa) ja garante que toda
    /// propriedade de ConfiguracaoCalculo e um Premissa&lt;T&gt;, entao essa premissa
    /// estrutural vale pra qualquer campo, presente ou futuro.
    /// </summary>
    public static bool PossuiPremissaProvisoria(this ConfiguracaoCalculo configuracao)
    {
        foreach (var propriedade in typeof(ConfiguracaoCalculo).GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            var premissa = propriedade.GetValue(configuracao)
                ?? throw new InvalidOperationException($"Propriedade '{propriedade.Name}' de ConfiguracaoCalculo esta nula.");
            var origemPropriedade = premissa.GetType().GetProperty(nameof(Premissa<object>.Origem))
                ?? throw new InvalidOperationException($"Propriedade '{propriedade.Name}' nao e um Premissa<T>.");
            var origem = (OrigemPremissa)origemPropriedade.GetValue(premissa)!;

            if (origem == OrigemPremissa.Provisorio)
            {
                return true;
            }
        }

        return false;
    }
}
