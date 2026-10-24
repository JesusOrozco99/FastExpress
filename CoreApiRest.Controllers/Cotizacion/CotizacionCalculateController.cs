using CoreApiRest.BusinessRules;
using CoreApiRest.BusinessRules.Interfaces.Controllers.Cotizacion;
using CoreApiRest.BusinessRules.Interfaces.Ports.Cotizacion;
using CoreApiRest.BusinessRules.Request.Cotizacion;
using CoreApiRest.BusinessRules.Responses;

namespace CoreApiRest.Controllers.Cotizacion;

public sealed class CotizacionCalculateController : ICotizacionCalculateController
{
    private readonly ICotizacionCalculateInputPort _inputPort;
    private readonly ICotizacionOutputPort _outputPort;

    public CotizacionCalculateController(ICotizacionCalculateInputPort inputPort, ICotizacionOutputPort outputPort)
    {
        ArgumentNullException.ThrowIfNull(inputPort);
        ArgumentNullException.ThrowIfNull(outputPort);
        _inputPort = inputPort;
        _outputPort = outputPort;
    }

    public async Task<Result<CalcularCotizacionResponse>> Calculate(CalcularCotizacionRequest request)
    {
        await _inputPort.Handle(request);
        return _outputPort.Result!;
    }
}
