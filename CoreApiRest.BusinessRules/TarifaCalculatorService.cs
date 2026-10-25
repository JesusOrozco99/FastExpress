namespace CoreApiRest.BusinessRules;

/// <summary>
/// Cálculo puro de la tarifa. El peso ya fue aceptado por BR-02.
/// Subtotal = 5.00 + (peso * 2.00 si peso &lt;= 5.00; peso * 4.00 si peso &gt; 5.00).
/// TarifaFinal = redondeo(subtotal * factorCliente * factorEnvio, 2).
/// </summary>
public sealed class TarifaCalculatorService
{
    public TarifaCotizacion CalcularTarifa(Peso peso, TipoCliente tipoCliente, TipoEnvio tipoEnvio)
    {
        ArgumentNullException.ThrowIfNull(peso);

        var tarifaPeso = peso.Valor * ResolverTarifaPorKilogramo(peso.Valor);
        var tarifaBase = Redondear(PoliticaTarifaria.CostoBase + tarifaPeso);
        var factorCliente = PoliticaTarifaria.FactoresCliente[tipoCliente];
        var multiplicadorEnvio = PoliticaTarifaria.MultiplicadoresEnvio[tipoEnvio];
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

    private static decimal ResolverTarifaPorKilogramo(decimal pesoKg) =>
        pesoKg <= PoliticaTarifaria.LimiteTramoLigero
            ? PoliticaTarifaria.TarifaKilogramoLigero
            : PoliticaTarifaria.TarifaKilogramoPesado;

    private static decimal Redondear(decimal valor) =>
        decimal.Round(valor, 2, MidpointRounding.AwayFromZero);
}
