using CoreApiRest.BusinessRules;
using CoreApiRest.BusinessRules.Interfaces.Ports.Cotizacion;
using CoreApiRest.BusinessRules.Responses;

namespace CoreApiRest.OutputPorts.Cotizacion;

public sealed class CotizacionOutputPort : ICotizacionOutputPort
{
    public Result<CalcularCotizacionResponse>? Result { get; private set; }

    public void Handle(Result<CalcularCotizacionResponse> result)
    {
        ArgumentNullException.ThrowIfNull(result);
        Result = result;
    }
}
