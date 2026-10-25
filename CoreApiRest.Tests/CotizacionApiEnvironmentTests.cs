using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;

namespace CoreApiRest.Tests;

public class CotizacionApiEnvironmentTests
{
    [Fact]
    public async Task Calculate_EnProduccion_RechazaElPesoFueraDeRangoCon422()
    {
        await using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder => builder.UseEnvironment(Environments.Production));
        var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var response = await client.PostAsJsonAsync("/api/v1/quotes/calculate", new
        {
            peso = 20.01m,
            tipoCliente = "Nuevo",
            tipoEnvio = "Express"
        });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }
}
