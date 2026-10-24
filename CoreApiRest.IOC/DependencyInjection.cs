using CoreApiRest.Controllers;
using CoreApiRest.OutputPorts;
using CoreApiRest.UseCases;
using Microsoft.Extensions.DependencyInjection;

namespace CoreApiRest.IOC;

public static class DependencyInjection
{
    public static IServiceCollection AddFastExpress(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddOutputPorts();
        services.AddUseCases();
        services.AddGranularControllers();

        return services;
    }
}
