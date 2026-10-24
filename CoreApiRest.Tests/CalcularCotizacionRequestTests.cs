using System.ComponentModel.DataAnnotations;
using CoreApiRest.BusinessRules;
using CoreApiRest.BusinessRules.Request.Cotizacion;

namespace CoreApiRest.Tests;

public class CalcularCotizacionRequestTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(0.01)]
    [InlineData(5)]
    [InlineData(20)]
    [InlineData(20.01)]
    public void Anotaciones_ElRangoDePesoNoEsUnErrorDeFormato(double peso)
    {
        var request = new CalcularCotizacionRequest
        {
            Peso = (decimal)peso,
            TipoCliente = TipoCliente.Regular,
            TipoEnvio = TipoEnvio.Estandar
        };

        var esValida = Validator.TryValidateObject(request, new ValidationContext(request), new List<ValidationResult>(), true);

        Assert.True(esValida);
    }

    [Fact]
    public void Anotaciones_CuandoFaltanDatosObligatorios_EsInvalida()
    {
        var request = new CalcularCotizacionRequest();
        var resultados = new List<ValidationResult>();

        var esValida = Validator.TryValidateObject(request, new ValidationContext(request), resultados, true);

        Assert.False(esValida);
        Assert.Contains(resultados, resultado => resultado.ErrorMessage == "El peso es obligatorio.");
        Assert.Contains(resultados, resultado => resultado.ErrorMessage == "El tipo de cliente es obligatorio.");
        Assert.Contains(resultados, resultado => resultado.ErrorMessage == "El tipo de envío es obligatorio.");
    }

    [Fact]
    public void Anotaciones_CuandoElTipoDeClienteNoEstaDefinido_EsInvalida()
    {
        var request = new CalcularCotizacionRequest
        {
            Peso = 5m,
            TipoCliente = (TipoCliente)99,
            TipoEnvio = TipoEnvio.Express
        };
        var resultados = new List<ValidationResult>();

        var esValida = Validator.TryValidateObject(request, new ValidationContext(request), resultados, true);

        Assert.False(esValida);
        Assert.Contains(resultados, resultado => resultado.ErrorMessage == "El tipo de cliente no tiene un formato válido.");
    }

    [Fact]
    public void Anotaciones_CuandoElTipoDeEnvioNoEstaDefinido_EsInvalida()
    {
        var request = new CalcularCotizacionRequest
        {
            Peso = 5m,
            TipoCliente = TipoCliente.Corporativo,
            TipoEnvio = (TipoEnvio)99
        };
        var resultados = new List<ValidationResult>();

        var esValida = Validator.TryValidateObject(request, new ValidationContext(request), resultados, true);

        Assert.False(esValida);
        Assert.Contains(resultados, resultado => resultado.ErrorMessage == "El tipo de envío no tiene un formato válido.");
    }
}
