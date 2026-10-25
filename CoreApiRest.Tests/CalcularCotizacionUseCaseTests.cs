using CoreApiRest.BusinessRules;
using CoreApiRest.BusinessRules.Request.Cotizacion;
using CoreApiRest.BusinessRules.Responses;
using CoreApiRest.OutputPorts.Cotizacion;
using CoreApiRest.UseCases.Cotizacion;

namespace CoreApiRest.Tests;

public class CalcularCotizacionUseCaseTests
{
    private readonly CotizacionOutputPort _outputPort = new();
    private readonly CalcularCotizacionUseCase _useCase;

    public CalcularCotizacionUseCaseTests()
    {
        _useCase = new CalcularCotizacionUseCase(new TarifaCalculatorService(), _outputPort);
    }

    public static TheoryData<decimal, bool, decimal?> ValoresLimite => new()
    {
        { 0m, false, null },
        { 0.01m, true, 5.02m },
        { 5.0m, true, 15.00m },
        { 20.0m, true, 85.00m },
        { 20.01m, false, null }
    };

    public static TheoryData<TipoCliente, TipoEnvio, decimal, decimal, decimal> Combinaciones => new()
    {
        { TipoCliente.Nuevo, TipoEnvio.Estandar, 0m, 1.00m, 15.00m },
        { TipoCliente.Nuevo, TipoEnvio.Express, 0m, 1.25m, 18.75m },
        { TipoCliente.Frecuente, TipoEnvio.Estandar, 10m, 1.00m, 13.50m },
        { TipoCliente.Frecuente, TipoEnvio.Express, 10m, 1.25m, 16.88m },
        { TipoCliente.VIP, TipoEnvio.Estandar, 20m, 1.00m, 12.00m },
        { TipoCliente.VIP, TipoEnvio.Express, 20m, 1.25m, 15.00m }
    };

    [Theory]
    [MemberData(nameof(ValoresLimite))]
    public async Task Handle_AnalisisDeValoresLimite_RespetaBr02(decimal pesoKg, bool esperadoExitoso, decimal? tarifaFinal)
    {
        var result = await Calcular(Solicitud(pesoKg, TipoCliente.Nuevo, TipoEnvio.Estandar));

        Assert.Equal(esperadoExitoso, result.IsSuccess);
        Assert.Equal(!esperadoExitoso, result.IsFailure);
        if (esperadoExitoso)
        {
            Assert.Null(result.Error);
            Assert.NotNull(result.Value);
            Assert.Equal(pesoKg, result.Value.PesoKg);
            Assert.Equal(tarifaFinal, result.Value.TarifaFinal);
        }
        else
        {
            Assert.Null(result.Value);
            Assert.NotNull(result.Error);
            Assert.Equal(CotizacionErrors.PesoFueraDeRango, result.Error.Code);
            Assert.Equal(ErrorType.BusinessRule, result.Error.Type);
            Assert.Contains("20.00 kg", result.Error.Message, StringComparison.Ordinal);
        }
    }

    [Theory]
    [MemberData(nameof(Combinaciones))]
    public async Task Handle_AplicaElAjusteDeClienteYElMultiplicadorDeEnvio(
        TipoCliente tipoCliente,
        TipoEnvio tipoEnvio,
        decimal porcentajeDescuento,
        decimal multiplicador,
        decimal tarifaFinal)
    {
        var result = await Calcular(Solicitud(5.0m, tipoCliente, tipoEnvio));

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(tipoCliente.ToString(), result.Value.TipoCliente);
        Assert.Equal(tipoEnvio.ToString(), result.Value.TipoEnvio);
        Assert.Equal(15.00m, result.Value.TarifaBase);
        Assert.Equal(porcentajeDescuento, result.Value.PorcentajeDescuentoCliente);
        Assert.Equal(multiplicador, result.Value.MultiplicadorEnvio);
        Assert.Equal(tarifaFinal, result.Value.TarifaFinal);
    }

    [Fact]
    public async Task Handle_CuandoLaSolicitudEsNula_RetornaValidacion()
    {
        var result = await Calcular(null!);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.Error!.Type);
        Assert.Equal(CotizacionErrors.SolicitudInvalida, result.Error.Code);
        Assert.Equal("La solicitud es obligatoria.", result.Error.Message);
    }

    [Fact]
    public async Task Handle_CuandoFaltaElPeso_RetornaValidacion()
    {
        var result = await Calcular(new CalcularCotizacionRequest
        {
            TipoCliente = TipoCliente.Nuevo,
            TipoEnvio = TipoEnvio.Estandar
        });

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.Error!.Type);
        Assert.Equal(CotizacionErrors.SolicitudInvalida, result.Error.Code);
        Assert.Equal("El peso es obligatorio.", result.Error.Message);
    }

    [Fact]
    public async Task Handle_CuandoFaltaElTipoDeCliente_RetornaValidacion()
    {
        var result = await Calcular(new CalcularCotizacionRequest
        {
            Peso = 5m,
            TipoEnvio = TipoEnvio.Estandar
        });

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.Error!.Type);
        Assert.Equal("El tipo de cliente es obligatorio.", result.Error.Message);
    }

    [Fact]
    public async Task Handle_CuandoFaltaElTipoDeEnvio_RetornaValidacion()
    {
        var result = await Calcular(new CalcularCotizacionRequest
        {
            Peso = 5m,
            TipoCliente = TipoCliente.Nuevo
        });

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.Error!.Type);
        Assert.Equal("El tipo de envío es obligatorio.", result.Error.Message);
    }

    [Fact]
    public async Task Handle_CuandoElTipoDeClienteNoEstaDefinido_RetornaBr03()
    {
        var result = await Calcular(Solicitud(5m, (TipoCliente)99, TipoEnvio.Estandar));

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.BusinessRule, result.Error!.Type);
        Assert.Equal(CotizacionErrors.TipoClienteNoSoportado, result.Error.Code);
    }

    [Fact]
    public async Task Handle_CuandoElTipoDeEnvioNoEstaDefinido_RetornaBr04()
    {
        var result = await Calcular(Solicitud(5m, TipoCliente.VIP, (TipoEnvio)99));

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.BusinessRule, result.Error!.Type);
        Assert.Equal(CotizacionErrors.TipoEnvioNoSoportado, result.Error.Code);
    }

    [Fact]
    public void Constructor_CuandoElCalculadorEsNulo_LanzaArgumentNullException()
    {
        var excepcion = Assert.Throws<ArgumentNullException>(() => new CalcularCotizacionUseCase(null!, _outputPort));

        Assert.Equal("tarifaCalculator", excepcion.ParamName);
    }

    [Fact]
    public void Constructor_CuandoElPuertoDeSalidaEsNulo_LanzaArgumentNullException()
    {
        var excepcion = Assert.Throws<ArgumentNullException>(() => new CalcularCotizacionUseCase(new TarifaCalculatorService(), null!));

        Assert.Equal("outputPort", excepcion.ParamName);
    }

    private async Task<Result<CalcularCotizacionResponse>> Calcular(CalcularCotizacionRequest request)
    {
        await _useCase.Handle(request);
        var result = _outputPort.Result;
        Assert.NotNull(result);
        return result;
    }

    private static CalcularCotizacionRequest Solicitud(decimal peso, TipoCliente tipoCliente, TipoEnvio tipoEnvio) =>
        new()
        {
            Peso = peso,
            TipoCliente = tipoCliente,
            TipoEnvio = tipoEnvio
        };
}
