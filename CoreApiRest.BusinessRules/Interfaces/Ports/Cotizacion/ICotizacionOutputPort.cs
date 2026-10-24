using CoreApiRest.BusinessRules.Responses;

namespace CoreApiRest.BusinessRules.Interfaces.Ports.Cotizacion;

public interface ICotizacionOutputPort
{
    void Handle(Result<CalcularCotizacionResponse> result);

    Result<CalcularCotizacionResponse>? Result { get; }
}
