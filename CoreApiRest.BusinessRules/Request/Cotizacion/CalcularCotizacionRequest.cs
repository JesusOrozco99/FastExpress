using System.ComponentModel.DataAnnotations;

namespace CoreApiRest.BusinessRules.Request.Cotizacion;

public sealed class CalcularCotizacionRequest
{
    [Required(ErrorMessage = "El peso es obligatorio.")]
    public decimal? Peso { get; set; }

    [Required(ErrorMessage = "El tipo de cliente es obligatorio.")]
    [EnumDataType(typeof(TipoCliente), ErrorMessage = "El tipo de cliente no tiene un formato válido.")]
    public TipoCliente? TipoCliente { get; set; }

    [Required(ErrorMessage = "El tipo de envío es obligatorio.")]
    [EnumDataType(typeof(TipoEnvio), ErrorMessage = "El tipo de envío no tiene un formato válido.")]
    public TipoEnvio? TipoEnvio { get; set; }
}
