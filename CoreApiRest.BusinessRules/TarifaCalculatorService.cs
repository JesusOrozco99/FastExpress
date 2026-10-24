namespace CoreApiRest.BusinessRules;

/// <summary>
/// Cálculo puro de la tarifa. No valida el rango de peso: recibe un <see cref="Peso"/> ya aceptado por BR-02.
/// TarifaBase = redondeo(pesoKg * tarifaPorKilogramo, 2).
/// TarifaFinal = redondeo(TarifaBase * factorCliente * multiplicadorEnvio, 2).
/// </summary>
public sealed class TarifaCalculatorService
{
    public TarifaCotizacion CalcularTarifa(Peso peso, TipoCliente tipoCliente, TipoEnvio tipoEnvio)
    {
        ArgumentNullException.ThrowIfNull(peso);

        var factorCliente = PoliticaTarifaria.FactoresCliente[tipoCliente];
        var multiplicadorEnvio = PoliticaTarifaria.MultiplicadoresEnvio[tipoEnvio];
        var tarifaBase = Redondear(peso.Valor * PoliticaTarifaria.TarifaPorKilogramo);
        var tarifaFinal = Redondear(tarifaBase * factorCliente * multiplicadorEnvio);

        return new TarifaCotizacion(
            peso.Valor,
            tipoCliente,
            tipoEnvio,
            tarifaBase,
            factorCliente,
            multiplicadorEnvio,
            tarifaFinal);
    }

    private static decimal Redondear(decimal valor) =>
        decimal.Round(valor, 2, MidpointRounding.AwayFromZero);
}
