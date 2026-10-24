using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace CoreApiRest.BusinessRules;

/// <summary>
/// Peso del paquete en kilogramos. BR-02: el valor debe ser mayor a 0 kg y menor o igual a 20.00 kg.
/// </summary>
public sealed class Peso
{
    private Peso(decimal valor) => Valor = valor;

    public decimal Valor { get; }

    public static bool TryCreate(
        decimal valor,
        [NotNullWhen(true)] out Peso? peso,
        [NotNullWhen(false)] out string? error)
    {
        if (valor <= PoliticaTarifaria.PesoMinimoExclusivo || valor > PoliticaTarifaria.PesoMaximoInclusivo)
        {
            peso = null;
            error = CrearMensajeDeRango(valor);
            return false;
        }

        peso = new Peso(valor);
        error = null;
        return true;
    }

    private static string CrearMensajeDeRango(decimal valor)
    {
        FormattableString mensaje =
            $"El peso debe ser mayor a {PoliticaTarifaria.PesoMinimoExclusivo:0.00} kg y menor o igual a {PoliticaTarifaria.PesoMaximoInclusivo:0.00} kg. Valor recibido: {valor:0.00} kg.";

        return mensaje.ToString(CultureInfo.InvariantCulture);
    }
}
