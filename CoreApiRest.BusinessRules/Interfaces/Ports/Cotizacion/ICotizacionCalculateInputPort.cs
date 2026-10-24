using CoreApiRest.BusinessRules.Request.Cotizacion;

namespace CoreApiRest.BusinessRules.Interfaces.Ports.Cotizacion;

public interface ICotizacionCalculateInputPort
{
    Task Handle(CalcularCotizacionRequest request);
}
