using CoreApiRest.BusinessRules;
using CoreApiRest.BusinessRules.Interfaces.Ports.Cotizacion;
using CoreApiRest.UseCases.Cotizacion;
using Microsoft.Extensions.DependencyInjection;

namespace CoreApiRest.UseCases;

public static class ServicesRegistration
{
    public static IServiceCollection AddUseCases(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<TarifaCalculatorService>();
        services.AddScoped<ICotizacionCalculateInputPort, CalcularCotizacionUseCase>();

        return services;
    }
}
