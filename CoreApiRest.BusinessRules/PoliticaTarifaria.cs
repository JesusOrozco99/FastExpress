namespace CoreApiRest.BusinessRules;

/// <summary>
/// Parámetros oficiales de cotización. BR-01 a BR-04 salen de aquí.
/// </summary>
public static class PoliticaTarifaria
{
    public const decimal CostoBase = 5.00m;

    public const decimal PesoMinimoExclusivo = 0m;
    public const decimal LimiteTramoLigero = 5.00m;
    public const decimal PesoMaximoInclusivo = 20.00m;

    public const decimal TarifaKilogramoLigero = 2.00m;
    public const decimal TarifaKilogramoPesado = 4.00m;

    public const decimal FactorNuevo = 1.00m;
    public const decimal FactorFrecuente = 0.90m;
    public const decimal FactorVip = 0.80m;

    public const decimal MultiplicadorEstandar = 1.00m;
    public const decimal MultiplicadorExpress = 1.25m;

    public static IReadOnlyDictionary<TipoCliente, decimal> FactoresCliente { get; } =
        new Dictionary<TipoCliente, decimal>
        {
            [TipoCliente.Nuevo] = FactorNuevo,
            [TipoCliente.Frecuente] = FactorFrecuente,
            [TipoCliente.VIP] = FactorVip
        };

    public static IReadOnlyDictionary<TipoEnvio, decimal> MultiplicadoresEnvio { get; } =
        new Dictionary<TipoEnvio, decimal>
        {
            [TipoEnvio.Estandar] = MultiplicadorEstandar,
            [TipoEnvio.Express] = MultiplicadorExpress
        };
}
