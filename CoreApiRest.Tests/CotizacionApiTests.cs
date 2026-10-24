using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using CoreApiRest.BusinessRules;
using CoreApiRest.BusinessRules.Responses;
using Microsoft.AspNetCore.Mvc.Testing;

namespace CoreApiRest.Tests;

public class CotizacionApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly HttpClient _client;

    public CotizacionApiTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    public static TheoryData<decimal, HttpStatusCode, decimal?> ValoresLimite => new()
    {
        { 0m, HttpStatusCode.UnprocessableEntity, null },
        { 0.01m, HttpStatusCode.OK, 0.05m },
        { 5.0m, HttpStatusCode.OK, 25.00m },
        { 20.0m, HttpStatusCode.OK, 100.00m },
        { 20.01m, HttpStatusCode.UnprocessableEntity, null }
    };

    [Theory]
    [MemberData(nameof(ValoresLimite))]
    public async Task Calculate_AnalisisDeValoresLimite_Mapea200YO422(decimal peso, HttpStatusCode estado, decimal? tarifaFinal)
    {
        var response = await _client.PostAsJsonAsync("/api/v1/quotes/calculate", new
        {
            peso,
            tipoCliente = "Regular",
            tipoEnvio = "Estandar"
        });

        Assert.Equal(estado, response.StatusCode);

        if (tarifaFinal is null)
        {
            var error = await response.Content.ReadFromJsonAsync<Error>(JsonOptions);
            Assert.NotNull(error);
            Assert.Equal("BR-02", error.Code);
            return;
        }

        var body = await response.Content.ReadFromJsonAsync<CalcularCotizacionResponse>(JsonOptions);
        Assert.NotNull(body);
        Assert.Equal(tarifaFinal, body.TarifaFinal);
        Assert.Equal("Regular", body.TipoCliente);
        Assert.Equal("Estandar", body.TipoEnvio);
    }

    [Theory]
    [InlineData("Corporativo", "Express", 33.75)]
    [InlineData("VIP", "Estandar", 20.00)]
    [InlineData("VIP", "Express", 30.00)]
    public async Task Calculate_CombinacionesDeClienteYEnvio_Retorna200(string tipoCliente, string tipoEnvio, decimal tarifaFinal)
    {
        var response = await _client.PostAsJsonAsync("/api/v1/quotes/calculate", new
        {
            peso = 5.0m,
            tipoCliente,
            tipoEnvio
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<CalcularCotizacionResponse>(JsonOptions);
        Assert.NotNull(body);
        Assert.Equal(tarifaFinal, body.TarifaFinal);
        Assert.Equal(tipoCliente, body.TipoCliente);
        Assert.Equal(tipoEnvio, body.TipoEnvio);
    }

    [Theory]
    [InlineData("{")]
    [InlineData("")]
    [InlineData("{}")]
    [InlineData("{\"peso\":\"abc\",\"tipoCliente\":\"Regular\",\"tipoEnvio\":\"Estandar\"}")]
    [InlineData("{\"peso\":5,\"tipoCliente\":\"Desconocido\",\"tipoEnvio\":\"Estandar\"}")]
    [InlineData("{\"peso\":5,\"tipoCliente\":\"Regular\",\"tipoEnvio\":\"Urgente\"}")]
    [InlineData("{\"peso\":5,\"tipoCliente\":99,\"tipoEnvio\":\"Estandar\"}")]
    public async Task Calculate_CuandoElJsonTieneErroresDeFormato_Retorna400(string json)
    {
        using var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/api/v1/quotes/calculate", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
