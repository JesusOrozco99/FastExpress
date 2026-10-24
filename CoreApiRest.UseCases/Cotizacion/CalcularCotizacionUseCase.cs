using System.Diagnostics.CodeAnalysis;
using CoreApiRest.BusinessRules;
using CoreApiRest.BusinessRules.Interfaces.Ports.Cotizacion;
using CoreApiRest.BusinessRules.Request.Cotizacion;
using CoreApiRest.BusinessRules.Responses;

namespace CoreApiRest.UseCases.Cotizacion;

public sealed class CalcularCotizacionUseCase : ICotizacionCalculateInputPort
{
    private readonly TarifaCalculatorService _tarifaCalculator;
    private readonly ICotizacionOutputPort _outputPort;

    public CalcularCotizacionUseCase(TarifaCalculatorService tarifaCalculator, ICotizacionOutputPort outputPort)
    {
        ArgumentNullException.ThrowIfNull(tarifaCalculator);
        ArgumentNullException.ThrowIfNull(outputPort);
        _tarifaCalculator = tarifaCalculator;
        _outputPort = outputPort;
    }

    public Task Handle(CalcularCotizacionRequest request)
    {
        if (!TryNormalizar(request, out var pesoKg, out var tipoCliente, out var tipoEnvio, out var error))
        {
            _outputPort.Handle(Result.Failure<CalcularCotizacionResponse>(error));
            return Task.CompletedTask;
        }

        if (!Peso.TryCreate(pesoKg, out var peso, out var errorPeso))
        {
            _outputPort.Handle(Result.Failure<CalcularCotizacionResponse>(CotizacionErrors.PesoInvalido(errorPeso)));
            return Task.CompletedTask;
        }

        var cotizacion = _tarifaCalculator.CalcularTarifa(peso, tipoCliente, tipoEnvio);
        _outputPort.Handle(Result.Success(Mapear(cotizacion)));
        return Task.CompletedTask;
    }

    private static bool TryNormalizar(
        CalcularCotizacionRequest? request,
        out decimal pesoKg,
        out TipoCliente tipoCliente,
        out TipoEnvio tipoEnvio,
        [NotNullWhen(false)] out Error? error)
    {
        pesoKg = default;
        tipoCliente = default;
        tipoEnvio = default;

        if (request is null)
        {
            error = CotizacionErrors.SolicitudObligatoria();
            return false;
        }

        if (request.Peso is null)
        {
            error = CotizacionErrors.PesoObligatorio();
            return false;
        }

        if (request.TipoCliente is null)
        {
            error = CotizacionErrors.TipoClienteObligatorio();
            return false;
        }

        if (request.TipoEnvio is null)
        {
            error = CotizacionErrors.TipoEnvioObligatorio();
            return false;
        }

        if (!Enum.IsDefined(request.TipoCliente.Value))
        {
            error = CotizacionErrors.ClienteNoSoportado();
            return false;
        }

        if (!Enum.IsDefined(request.TipoEnvio.Value))
        {
            error = CotizacionErrors.EnvioNoSoportado();
            return false;
        }

        pesoKg = request.Peso.Value;
        tipoCliente = request.TipoCliente.Value;
        tipoEnvio = request.TipoEnvio.Value;
        error = null;
        return true;
    }

    private static CalcularCotizacionResponse Mapear(TarifaCotizacion cotizacion) =>
        new()
        {
            PesoKg = cotizacion.PesoKg,
            TipoCliente = cotizacion.TipoCliente.ToString(),
            TipoEnvio = cotizacion.TipoEnvio.ToString(),
            TarifaBase = cotizacion.TarifaBase,
            FactorCliente = cotizacion.FactorCliente,
            PorcentajeDescuentoCliente = decimal.Round((1m - cotizacion.FactorCliente) * 100m, 2, MidpointRounding.AwayFromZero),
            MultiplicadorEnvio = cotizacion.MultiplicadorEnvio,
            TarifaFinal = cotizacion.TarifaFinal
        };
}
