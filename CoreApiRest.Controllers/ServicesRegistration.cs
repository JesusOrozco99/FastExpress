using CoreApiRest.BusinessRules.Interfaces.Controllers.Cotizacion;
using CoreApiRest.Controllers.Cotizacion;
using Microsoft.Extensions.DependencyInjection;

namespace CoreApiRest.Controllers;

public static class ServicesRegistration
{
    public static IServiceCollection AddGranularControllers(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddScoped<ICotizacionCalculateController, CotizacionCalculateController>();

        return services;
    }
}
