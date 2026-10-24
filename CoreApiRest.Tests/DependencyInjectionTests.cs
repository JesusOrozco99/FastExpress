using CoreApiRest.BusinessRules;
using CoreApiRest.BusinessRules.Interfaces.Controllers.Cotizacion;
using CoreApiRest.BusinessRules.Interfaces.Ports.Cotizacion;
using CoreApiRest.BusinessRules.Request.Cotizacion;
using CoreApiRest.Controllers.Cotizacion;
using CoreApiRest.IOC;
using CoreApiRest.UseCases.Cotizacion;
using Microsoft.Extensions.DependencyInjection;

namespace CoreApiRest.Tests;

public class DependencyInjectionTests
{
    [Fact]
    public async Task AddFastExpress_RegistraElCalculadorComoSingletonYElCasoDeUsoComoScoped()
    {
        var services = new ServiceCollection();

        var returned = services.AddFastExpress();

        Assert.Same(services, returned);
        using var provider = services.BuildServiceProvider();
        using var primerScope = provider.CreateScope();
        using var segundoScope = provider.CreateScope();

        var calculadorRaiz = provider.GetRequiredService<TarifaCalculatorService>();
        var calculadorScope = primerScope.ServiceProvider.GetRequiredService<TarifaCalculatorService>();
        var primerCaso = primerScope.ServiceProvider.GetRequiredService<ICotizacionCalculateInputPort>();
        var segundoCaso = segundoScope.ServiceProvider.GetRequiredService<ICotizacionCalculateInputPort>();
        var primerPuerto = primerScope.ServiceProvider.GetRequiredService<ICotizacionOutputPort>();
        var segundoPuerto = segundoScope.ServiceProvider.GetRequiredService<ICotizacionOutputPort>();
        var controlador = primerScope.ServiceProvider.GetRequiredService<ICotizacionCalculateController>();

        Assert.IsType<CalcularCotizacionUseCase>(primerCaso);
        Assert.IsType<CotizacionCalculateController>(controlador);
        Assert.Same(calculadorRaiz, calculadorScope);
        Assert.NotSame(primerCaso, segundoCaso);
        Assert.NotSame(primerPuerto, segundoPuerto);

        var result = await controlador.Calculate(new CalcularCotizacionRequest
        {
            Peso = 5.0m,
            TipoCliente = TipoCliente.Regular,
            TipoEnvio = TipoEnvio.Estandar
        });

        Assert.True(result.IsSuccess);
        Assert.Equal(25.00m, result.Value!.TarifaFinal);
        Assert.Same(result, primerPuerto.Result);
    }

    [Fact]
    public void AddFastExpress_CuandoLaColeccionEsNula_LanzaArgumentNullException()
    {
        IServiceCollection? services = null;

        var excepcion = Assert.Throws<ArgumentNullException>(() => services!.AddFastExpress());

        Assert.Equal("services", excepcion.ParamName);
    }
}
