namespace CoreApiRest.BusinessRules;

/// <summary>
/// BR-04. Recargo del tipo de envío sobre el subtotal ya descontado.
/// Estándar no recarga y Express recarga 25%.
/// </summary>
public enum TipoEnvio
{
    Estandar = 1,
    Express = 2
}
