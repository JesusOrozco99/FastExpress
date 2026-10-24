using CoreApiRest.BusinessRules.Interfaces.Ports.Cotizacion;
using CoreApiRest.OutputPorts.Cotizacion;
using Microsoft.Extensions.DependencyInjection;

namespace CoreApiRest.OutputPorts;

public static class ServicesRegistration
{
    public static IServiceCollection AddOutputPorts(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddScoped<ICotizacionOutputPort, CotizacionOutputPort>();

        return services;
    }
}
