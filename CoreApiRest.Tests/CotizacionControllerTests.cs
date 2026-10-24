using System.Security.Claims;
using CoreApiRest.BusinessRules;
using CoreApiRest.BusinessRules.Interfaces.Controllers.Cotizacion;
using CoreApiRest.BusinessRules.Request.Cotizacion;
using CoreApiRest.BusinessRules.Responses;
using CoreApiRest.Controllers.Cotizacion;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CoreApiRest.Tests;

public class CotizacionControllerTests
{
    [Fact]
    public async Task Calculate_CuandoElCasoDeUsoTieneExito_Retorna200()
    {
        var respuesta = new CalcularCotizacionResponse { PesoKg = 5m, TarifaFinal = 25m };
        var controlador = CrearControlador(Result.Success(respuesta), "sub", "usuario-1");

        var accion = await controlador.Calculate(new CalcularCotizacionRequest());

        var ok = Assert.IsType<OkObjectResult>(accion);
        Assert.Equal(StatusCodes.Status200OK, ok.StatusCode);
        Assert.Same(respuesta, ok.Value);
    }

    [Fact]
    public async Task Calculate_CuandoFallaLaReglaDeNegocio_Retorna422()
    {
        var error = new Error(
            CotizacionErrors.PesoFueraDeRango,
            "El peso debe ser mayor a 0.00 kg y menor o igual a 20.00 kg. Valor recibido: 20.01 kg.",
            ErrorType.BusinessRule);
        var controlador = CrearControlador(Result.Failure<CalcularCotizacionResponse>(error), ClaimTypes.Actor, "actor-1");

        var accion = await controlador.Calculate(new CalcularCotizacionRequest());

        var unprocessable = Assert.IsType<UnprocessableEntityObjectResult>(accion);
        Assert.Equal(StatusCodes.Status422UnprocessableEntity, unprocessable.StatusCode);
        Assert.Same(error, unprocessable.Value);
    }

    [Theory]
    [InlineData(ErrorType.Validation, StatusCodes.Status400BadRequest)]
    [InlineData(ErrorType.Conflict, StatusCodes.Status409Conflict)]
    [InlineData(ErrorType.NotFound, StatusCodes.Status404NotFound)]
    public async Task Calculate_MapeaElTipoDeErrorAlEstadoHttp(ErrorType tipo, int estado)
    {
        var error = new Error("CODIGO", "detalle", tipo);
        var controlador = CrearControlador(Result.Failure<CalcularCotizacionResponse>(error));

        var accion = await controlador.Calculate(new CalcularCotizacionRequest());

        var objectResult = Assert.IsAssignableFrom<ObjectResult>(accion);
        Assert.Equal(estado, objectResult.StatusCode);
        Assert.Same(error, objectResult.Value);
    }

    [Fact]
    public async Task Calculate_CuandoElTipoDeErrorNoEstaDefinido_Retorna500()
    {
        var error = new Error("X", "inesperado", (ErrorType)99);
        var controlador = CrearControlador(Result.Failure<CalcularCotizacionResponse>(error));

        var accion = await controlador.Calculate(new CalcularCotizacionRequest());

        var objectResult = Assert.IsAssignableFrom<ObjectResult>(accion);
        Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
    }

    [Fact]
    public void Constructor_CuandoElControladorGranularEsNulo_LanzaArgumentNullException()
    {
        var excepcion = Assert.Throws<ArgumentNullException>(() => new CotizacionController(null!));

        Assert.Equal("calculateController", excepcion.ParamName);
    }

    private static CotizacionController CrearControlador(
        Result<CalcularCotizacionResponse> resultado,
        string? claimType = null,
        string? claimValue = null)
    {
        var controlador = new CotizacionController(new ControladorFijo(resultado));
        var httpContext = new DefaultHttpContext();
        if (claimType is not null && claimValue is not null)
        {
            httpContext.User = new ClaimsPrincipal(
                new ClaimsIdentity([new Claim(claimType, claimValue)], "Test"));
        }

        controlador.ControllerContext = new ControllerContext { HttpContext = httpContext };
        return controlador;
    }

    private sealed class ControladorFijo : ICotizacionCalculateController
    {
        private readonly Result<CalcularCotizacionResponse> _resultado;

        public ControladorFijo(Result<CalcularCotizacionResponse> resultado) => _resultado = resultado;

        public Task<Result<CalcularCotizacionResponse>> Calculate(CalcularCotizacionRequest request) =>
            Task.FromResult(_resultado);
    }
}
