namespace CoreApiRest.BusinessRules;

/// <summary>
/// BR-03. Tipo de cliente que ajusta la tarifa base mediante un factor de descuento.
/// Regular no descuenta, Corporativo descuenta 10% y VIP descuenta 20%.
/// </summary>
public enum TipoCliente
{
    Regular = 1,
    Corporativo = 2,
    VIP = 3
}
