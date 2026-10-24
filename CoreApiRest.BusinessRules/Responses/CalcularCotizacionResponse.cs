namespace CoreApiRest.BusinessRules.Responses;

public sealed class CalcularCotizacionResponse
{
    public decimal PesoKg { get; init; }

    public string TipoCliente { get; init; } = string.Empty;

    public string TipoEnvio { get; init; } = string.Empty;

    public decimal TarifaBase { get; init; }

    public decimal FactorCliente { get; init; }

    public decimal PorcentajeDescuentoCliente { get; init; }

    public decimal MultiplicadorEnvio { get; init; }

    public decimal TarifaFinal { get; init; }
}
