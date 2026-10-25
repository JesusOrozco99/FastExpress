namespace CoreApiRest.BusinessRules;

/// <summary>
/// BR-03. Descuento del cliente sobre el subtotal.
/// Nuevo no descuenta, Frecuente descuenta 10% y VIP descuenta 20%.
/// </summary>
public enum TipoCliente
{
    Nuevo = 1,
    Frecuente = 2,
    VIP = 3
}
