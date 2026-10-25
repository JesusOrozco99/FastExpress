using CoreApiRest.BusinessRules;
using CoreApiRest.BusinessRules.Request.Cotizacion;
using CoreApiRest.BusinessRules.Responses;
using CoreApiRest.OutputPorts.Cotizacion;
using CoreApiRest.UseCases.Cotizacion;

namespace CoreApiRest.Tests;

/// <summary>
/// Pruebas de caja blanca y verificación de reglas de negocio
/// para el flujo de cotización de FastExpress.
///
/// Requisitos de referencia: documento del profesor.
/// BR-01: tarifa base de $5.00.
/// BR-02: $2.00/kg entre 0.01 y 5.00 kg;
///        $4.00/kg entre 5.01 y 20.00 kg;
///        rechazo por encima de 20.00 kg.
/// BR-03: NUEVO 0 %, FRECUENTE 10 %, VIP 20 %.
/// BR-04: ESTANDAR 0 %, EXPRESS 25 %.
///
/// NOTA:
/// Estos casos verifican el comportamiento requerido.
/// El backend actual puede fallar algunos casos hasta que
/// se corrijan las diferencias con la especificación.
/// </summary>
[Trait("Categoria", "CajaBlanca")]
public class WhiteBoxTests
{
    // ---------------------------------------------------------
    // TC-WB-01: Solicitud nula
    // ---------------------------------------------------------

    [Fact(DisplayName = "TC-WB-01 | Solicitud nula")]
    public async Task TCWB01_RequestNulo_RetornaValidacion()
    {
        var result = await Ejecutar(null!);

        AssertFallo(
            result,
            ErrorType.Validation,
            CotizacionErrors.SolicitudInvalida);

        Assert.Equal(
            "La solicitud es obligatoria.",
            result.Error!.Message);
    }

    // ---------------------------------------------------------
    // TC-WB-02: Peso nulo
    // ---------------------------------------------------------

    [Fact(DisplayName = "TC-WB-02 | Peso nulo")]
    public async Task TCWB02_PesoNulo_RetornaValidacion()
    {
        var result = await Ejecutar(new CalcularCotizacionRequest
        {
            Peso = null,
            TipoCliente = ClienteNuevo(),
            TipoEnvio = TipoEnvio.Estandar
        });

        AssertFallo(
            result,
            ErrorType.Validation,
            CotizacionErrors.SolicitudInvalida);

        Assert.Equal(
            "El peso es obligatorio.",
            result.Error!.Message);
    }

    // ---------------------------------------------------------
    // TC-WB-03: Tipo de cliente nulo
    // ---------------------------------------------------------

    [Fact(DisplayName = "TC-WB-03 | Tipo de cliente nulo")]
    public async Task TCWB03_TipoClienteNulo_RetornaValidacion()
    {
        var result = await Ejecutar(new CalcularCotizacionRequest
        {
            Peso = 5.00m,
            TipoCliente = null,
            TipoEnvio = TipoEnvio.Estandar
        });

        AssertFallo(
            result,
            ErrorType.Validation,
            CotizacionErrors.SolicitudInvalida);

        Assert.Equal(
            "El tipo de cliente es obligatorio.",
            result.Error!.Message);
    }

    // ---------------------------------------------------------
    // TC-WB-04: Tipo de envío nulo
    // ---------------------------------------------------------

    [Fact(DisplayName = "TC-WB-04 | Tipo de envío nulo")]
    public async Task TCWB04_TipoEnvioNulo_RetornaValidacion()
    {
        var result = await Ejecutar(new CalcularCotizacionRequest
        {
            Peso = 5.00m,
            TipoCliente = ClienteNuevo(),
            TipoEnvio = null
        });

        AssertFallo(
            result,
            ErrorType.Validation,
            CotizacionErrors.SolicitudInvalida);

        Assert.Equal(
            "El tipo de envío es obligatorio.",
            result.Error!.Message);
    }

    // ---------------------------------------------------------
    // TC-WB-05: Tipo de cliente no definido en el enum
    // ---------------------------------------------------------

    [Fact(DisplayName = "TC-WB-05 | Cliente no soportado")]
    public async Task TCWB05_TipoClienteNoDefinido_RetornaBr03()
    {
        var result = await Ejecutar(
            Solicitud(5.00m, (TipoCliente)99, TipoEnvio.Estandar));

        AssertFallo(
            result,
            ErrorType.BusinessRule,
            CotizacionErrors.TipoClienteNoSoportado);
    }

    // ---------------------------------------------------------
    // TC-WB-06: Tipo de envío no definido en el enum
    // ---------------------------------------------------------

    [Fact(DisplayName = "TC-WB-06 | Envío no soportado")]
    public async Task TCWB06_TipoEnvioNoDefinido_RetornaBr04()
    {
        var result = await Ejecutar(
            Solicitud(5.00m, ClienteVip(), (TipoEnvio)99));

        AssertFallo(
            result,
            ErrorType.BusinessRule,
            CotizacionErrors.TipoEnvioNoSoportado);
    }

    // ---------------------------------------------------------
    // TC-WB-07: Peso cero
    // ---------------------------------------------------------

    [Fact(DisplayName = "TC-WB-07 | Peso cero rechazado")]
    public async Task TCWB07_PesoCero_RetornaBr02()
    {
        var result = await Ejecutar(
            Solicitud(0.00m, ClienteNuevo(), TipoEnvio.Estandar));

        AssertFallo(
            result,
            ErrorType.BusinessRule,
            CotizacionErrors.PesoFueraDeRango);

        Assert.Contains(
            "Valor recibido: 0.00 kg",
            result.Error!.Message,
            StringComparison.Ordinal);
    }

    // ---------------------------------------------------------
    // TC-WB-08: Peso superior al máximo
    // ---------------------------------------------------------

    [Fact(DisplayName = "TC-WB-08 | Peso 20.01 kg rechazado")]
    public async Task TCWB08_PesoSuperaElMaximo_RetornaBr02()
    {
        var result = await Ejecutar(
            Solicitud(20.01m, ClienteNuevo(), TipoEnvio.Estandar));

        AssertFallo(
            result,
            ErrorType.BusinessRule,
            CotizacionErrors.PesoFueraDeRango);

        Assert.Contains(
            "Valor recibido: 20.01 kg",
            result.Error!.Message,
            StringComparison.Ordinal);
    }

    // ---------------------------------------------------------
    // TC-WB-09: Tramo ligero, límite superior
    // 5 + (5.00 * 2) = 15.00
    // ---------------------------------------------------------

    [Fact(DisplayName = "TC-WB-09 | Tramo ligero de 5.00 kg")]
    public async Task TCWB09_TramoLigero_CalculaTarifaEsperada()
    {
        var result = await Ejecutar(
            Solicitud(5.00m, ClienteNuevo(), TipoEnvio.Estandar));

        AssertExito(result);

        Assert.Equal(5.00m, result.Value!.PesoKg);
        Assert.Equal(15.00m, result.Value.TarifaBase);
        Assert.Equal(15.00m, result.Value.TarifaFinal);
    }

    // ---------------------------------------------------------
    // TC-WB-10: Tramo pesado, límite inferior
    // 5 + (5.01 * 4) = 25.04
    // ---------------------------------------------------------

    [Fact(DisplayName = "TC-WB-10 | Tramo pesado de 5.01 kg")]
    public async Task TCWB10_TramoPesado_CalculaTarifaEsperada()
    {
        var result = await Ejecutar(
            Solicitud(5.01m, ClienteNuevo(), TipoEnvio.Estandar));

        AssertExito(result);

        Assert.Equal(5.01m, result.Value!.PesoKg);
        Assert.Equal(25.04m, result.Value.TarifaBase);
        Assert.Equal(25.04m, result.Value.TarifaFinal);
    }

    // ---------------------------------------------------------
    // TC-WB-11: Peso negativo
    // ---------------------------------------------------------

    [Fact(DisplayName = "TC-WB-11 | Peso negativo rechazado")]
    public async Task TCWB11_PesoNegativo_RetornaBr02()
    {
        var result = await Ejecutar(
            Solicitud(-0.01m, ClienteNuevo(), TipoEnvio.Estandar));

        AssertFallo(
            result,
            ErrorType.BusinessRule,
            CotizacionErrors.PesoFueraDeRango);
    }

    // ---------------------------------------------------------
    // TC-WB-12: Peso mínimo permitido
    // 5 + (0.01 * 2) = 5.02
    // ---------------------------------------------------------

    [Fact(DisplayName = "TC-WB-12 | Peso mínimo de 0.01 kg")]
    public async Task TCWB12_PesoMinimo_CalculaTarifaEsperada()
    {
        var result = await Ejecutar(
            Solicitud(0.01m, ClienteNuevo(), TipoEnvio.Estandar));

        AssertExito(result);

        Assert.Equal(0.01m, result.Value!.PesoKg);
        Assert.Equal(5.02m, result.Value.TarifaBase);
        Assert.Equal(5.02m, result.Value.TarifaFinal);
    }

    // ---------------------------------------------------------
    // TC-WB-13: Peso máximo permitido
    // 5 + (20.00 * 4) = 85.00
    // ---------------------------------------------------------

    [Fact(DisplayName = "TC-WB-13 | Peso máximo de 20.00 kg")]
    public async Task TCWB13_PesoMaximo_CalculaTarifaEsperada()
    {
        var result = await Ejecutar(
            Solicitud(20.00m, ClienteNuevo(), TipoEnvio.Estandar));

        AssertExito(result);

        Assert.Equal(20.00m, result.Value!.PesoKg);
        Assert.Equal(85.00m, result.Value.TarifaBase);
        Assert.Equal(85.00m, result.Value.TarifaFinal);
    }

    // ---------------------------------------------------------
    // TC-WB-14: Cliente NUEVO, envío ESTANDAR
    // 1 kg: subtotal = 5 + (1 * 2) = 7.00
    // Sin descuento y sin recargo: total = 7.00
    // ---------------------------------------------------------

    [Fact(DisplayName = "TC-WB-14 | Nuevo y estándar")]
    public async Task TCWB14_NuevoEstandar_SinDescuentoNiRecargo()
    {
        var result = await Ejecutar(
            Solicitud(1.00m, ClienteNuevo(), TipoEnvio.Estandar));

        AssertExito(result);

        Assert.Equal(7.00m, result.Value!.TarifaBase);
        Assert.Equal(7.00m, result.Value.TarifaFinal);
    }

    // ---------------------------------------------------------
    // TC-WB-15: Cliente NUEVO, envío EXPRESS
    // 7.00 * 1.25 = 8.75
    // ---------------------------------------------------------

    [Fact(DisplayName = "TC-WB-15 | Nuevo y express")]
    public async Task TCWB15_NuevoExpress_AplicaRecargoDel25PorCiento()
    {
        var result = await Ejecutar(
            Solicitud(1.00m, ClienteNuevo(), TipoEnvio.Express));

        AssertExito(result);

        Assert.Equal(7.00m, result.Value!.TarifaBase);
        Assert.Equal(8.75m, result.Value.TarifaFinal);
    }

    // ---------------------------------------------------------
    // TC-WB-16: Cliente FRECUENTE, envío ESTANDAR
    // 7.00 * 0.90 = 6.30
    // ---------------------------------------------------------

    [Fact(DisplayName = "TC-WB-16 | Frecuente y estándar")]
    public async Task TCWB16_FrecuenteEstandar_AplicaDescuentoDel10PorCiento()
    {
        var result = await Ejecutar(
            Solicitud(1.00m, ClienteFrecuente(), TipoEnvio.Estandar));

        AssertExito(result);

        Assert.Equal(7.00m, result.Value!.TarifaBase);
        Assert.Equal(6.30m, result.Value.TarifaFinal);
    }

    // ---------------------------------------------------------
    // TC-WB-17: Cliente FRECUENTE, envío EXPRESS
    // 7.00 * 0.90 * 1.25 = 7.875 -> 7.88
    // Se supone descuento antes del recargo; confirmar orden
    // de operaciones con el profesor.
    // ---------------------------------------------------------

    [Fact(DisplayName = "TC-WB-17 | Frecuente y express")]
    public async Task TCWB17_FrecuenteExpress_AplicaDescuentoYRecargo()
    {
        var result = await Ejecutar(
            Solicitud(1.00m, ClienteFrecuente(), TipoEnvio.Express));

        AssertExito(result);

        Assert.Equal(7.00m, result.Value!.TarifaBase);
        Assert.Equal(7.88m, result.Value.TarifaFinal);
    }

    // ---------------------------------------------------------
    // TC-WB-18: Cliente VIP, envío ESTANDAR
    // 7.00 * 0.80 = 5.60
    // ---------------------------------------------------------

    [Fact(DisplayName = "TC-WB-18 | VIP y estándar")]
    public async Task TCWB18_VipEstandar_AplicaDescuentoDel20PorCiento()
    {
        var result = await Ejecutar(
            Solicitud(1.00m, ClienteVip(), TipoEnvio.Estandar));

        AssertExito(result);

        Assert.Equal(7.00m, result.Value!.TarifaBase);
        Assert.Equal(5.60m, result.Value.TarifaFinal);
    }

    // ---------------------------------------------------------
    // TC-WB-19: Cliente VIP, envío EXPRESS
    // 7.00 * 0.80 * 1.25 = 7.00
    // ---------------------------------------------------------

    [Fact(DisplayName = "TC-WB-19 | VIP y express")]
    public async Task TCWB19_VipExpress_AplicaDescuentoYRecargo()
    {
        var result = await Ejecutar(
            Solicitud(1.00m, ClienteVip(), TipoEnvio.Express));

        AssertExito(result);

        Assert.Equal(7.00m, result.Value!.TarifaBase);
        Assert.Equal(7.00m, result.Value.TarifaFinal);
    }

    // ---------------------------------------------------------
    // Helpers
    // ---------------------------------------------------------

    private static async Task<Result<CalcularCotizacionResponse>> Ejecutar(
        CalcularCotizacionRequest request)
    {
        // Se crea una salida nueva por prueba para evitar reutilizar
        // el resultado de una ejecución anterior.
        var outputPort = new CotizacionOutputPort();

        var useCase = new CalcularCotizacionUseCase(
            new TarifaCalculatorService(),
            outputPort);

        await useCase.Handle(request);

        var result = outputPort.Result;

        Assert.NotNull(result);

        return result;
    }

    private static CalcularCotizacionRequest Solicitud(
        decimal peso,
        TipoCliente tipoCliente,
        TipoEnvio tipoEnvio) =>
        new()
        {
            Peso = peso,
            TipoCliente = tipoCliente,
            TipoEnvio = tipoEnvio
        };

    private static TipoCliente ClienteNuevo() =>
        Enum.Parse<TipoCliente>("Nuevo", ignoreCase: true);

    private static TipoCliente ClienteFrecuente() =>
        Enum.Parse<TipoCliente>("Frecuente", ignoreCase: true);

    private static TipoCliente ClienteVip() =>
        Enum.Parse<TipoCliente>("VIP", ignoreCase: true);

    private static void AssertFallo(
        Result<CalcularCotizacionResponse> result,
        ErrorType tipo,
        string codigo)
    {
        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.NotNull(result.Error);
        Assert.Equal(tipo, result.Error.Type);
        Assert.Equal(codigo, result.Error.Code);
    }

    private static void AssertExito(
        Result<CalcularCotizacionResponse> result)
    {
        Assert.True(result.IsSuccess);
        Assert.Null(result.Error);
        Assert.NotNull(result.Value);
    }
}