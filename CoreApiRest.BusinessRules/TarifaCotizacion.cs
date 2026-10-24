namespace CoreApiRest.BusinessRules;

/// <summary>
/// Desglose de una cotización ya calculada con peso válido y tipos soportados.
/// </summary>
public sealed class TarifaCotizacion
{
    public TarifaCotizacion(
        decimal pesoKg,
        TipoCliente tipoCliente,
        TipoEnvio tipoEnvio,
        decimal tarifaBase,
        decimal factorCliente,
        decimal multiplicadorEnvio,
        decimal tarifaFinal)
    {
        PesoKg = pesoKg;
        TipoCliente = tipoCliente;
        TipoEnvio = tipoEnvio;
        TarifaBase = tarifaBase;
        FactorCliente = factorCliente;
        MultiplicadorEnvio = multiplicadorEnvio;
        TarifaFinal = tarifaFinal;
    }

    public decimal PesoKg { get; }

    public TipoCliente TipoCliente { get; }

    public TipoEnvio TipoEnvio { get; }

    public decimal TarifaBase { get; }

    public decimal FactorCliente { get; }

    public decimal MultiplicadorEnvio { get; }

    public decimal TarifaFinal { get; }
}
