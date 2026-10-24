namespace CoreApiRest.BusinessRules;

/// <summary>
/// BR-04. Tipo de envío que multiplica la tarifa base.
/// Estándar conserva la tarifa y Express la multiplica por 1.50.
/// </summary>
public enum TipoEnvio
{
    Estandar = 1,
    Express = 2
}
