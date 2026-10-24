namespace CoreApiRest.BusinessRules;

/// <summary>
/// Parámetros de la política de cotización. Es la única fuente de los factores BR-03 y BR-04.
/// </summary>
public static class PoliticaTarifaria
{
    public const decimal TarifaPorKilogramo = 5.00m;

    public const decimal PesoMinimoExclusivo = 0m;
    public const decimal PesoMaximoInclusivo = 20.00m;

    public const decimal FactorRegular = 1.00m;
    public const decimal FactorCorporativo = 0.90m;
    public const decimal FactorVip = 0.80m;

    public const decimal MultiplicadorEstandar = 1.00m;
    public const decimal MultiplicadorExpress = 1.50m;

    public static IReadOnlyDictionary<TipoCliente, decimal> FactoresCliente { get; } =
        new Dictionary<TipoCliente, decimal>
        {
            [TipoCliente.Regular] = FactorRegular,
            [TipoCliente.Corporativo] = FactorCorporativo,
            [TipoCliente.VIP] = FactorVip
        };

    public static IReadOnlyDictionary<TipoEnvio, decimal> MultiplicadoresEnvio { get; } =
        new Dictionary<TipoEnvio, decimal>
        {
            [TipoEnvio.Estandar] = MultiplicadorEstandar,
            [TipoEnvio.Express] = MultiplicadorExpress
        };
}
