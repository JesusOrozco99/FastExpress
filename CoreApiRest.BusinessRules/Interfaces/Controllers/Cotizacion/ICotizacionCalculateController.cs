using CoreApiRest.BusinessRules.Request.Cotizacion;
using CoreApiRest.BusinessRules.Responses;

namespace CoreApiRest.BusinessRules.Interfaces.Controllers.Cotizacion;

public interface ICotizacionCalculateController
{
    Task<Result<CalcularCotizacionResponse>> Calculate(CalcularCotizacionRequest request);
}
