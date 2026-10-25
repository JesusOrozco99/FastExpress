using CoreApiRest.BusinessRules;

namespace CoreApiRest.Tests;

public class TarifaCalculatorServiceTests
{
    private readonly TarifaCalculatorService _service = new();

    public static TheoryData<decimal, TipoCliente, TipoEnvio, decimal, decimal, decimal, decimal> Combinaciones => new()
    {
        { 5.0m, TipoCliente.Nuevo, TipoEnvio.Estandar, 15.00m, 1.00m, 1.00m, 15.00m },
        { 5.0m, TipoCliente.Nuevo, TipoEnvio.Express, 15.00m, 1.00m, 1.25m, 18.75m },
        { 5.0m, TipoCliente.Frecuente, TipoEnvio.Estandar, 15.00m, 0.90m, 1.00m, 13.50m },
        { 5.0m, TipoCliente.Frecuente, TipoEnvio.Express, 15.00m, 0.90m, 1.25m, 16.88m },
        { 5.0m, TipoCliente.VIP, TipoEnvio.Estandar, 15.00m, 0.80m, 1.00m, 12.00m },
        { 1.0m, TipoCliente.VIP, TipoEnvio.Express, 7.00m, 0.80m, 1.25m, 7.00m },
        { 0.01m, TipoCliente.Nuevo, TipoEnvio.Estandar, 5.02m, 1.00m, 1.00m, 5.02m },
        { 20.0m, TipoCliente.VIP, TipoEnvio.Express, 85.00m, 0.80m, 1.25m, 85.00m }
    };

    [Fact]
    public void Politica_CubreTodosLosTiposDeClienteYEnvio()
    {
        Assert.Equal(3, PoliticaTarifaria.FactoresCliente.Count);
        Assert.Equal(2, PoliticaTarifaria.MultiplicadoresEnvio.Count);

        foreach (var tipo in Enum.GetValues<TipoCliente>())
            Assert.True(PoliticaTarifaria.FactoresCliente.ContainsKey(tipo));

        foreach (var tipo in Enum.GetValues<TipoEnvio>())
            Assert.True(PoliticaTarifaria.MultiplicadoresEnvio.ContainsKey(tipo));
    }

    [Theory]
    [MemberData(nameof(Combinaciones))]
    public void CalcularTarifa_AplicaDescuentoDeClienteYMultiplicadorDeEnvio(
        decimal pesoKg,
        TipoCliente tipoCliente,
        TipoEnvio tipoEnvio,
        decimal tarifaBase,
        decimal factorCliente,
        decimal multiplicadorEnvio,
        decimal tarifaFinal)
    {
        Assert.True(Peso.TryCreate(pesoKg, out var peso, out _));

        var cotizacion = _service.CalcularTarifa(peso!, tipoCliente, tipoEnvio);

        Assert.Equal(pesoKg, cotizacion.PesoKg);
        Assert.Equal(tipoCliente, cotizacion.TipoCliente);
        Assert.Equal(tipoEnvio, cotizacion.TipoEnvio);
        Assert.Equal(tarifaBase, cotizacion.TarifaBase);
        Assert.Equal(factorCliente, cotizacion.FactorCliente);
        Assert.Equal(multiplicadorEnvio, cotizacion.MultiplicadorEnvio);
        Assert.Equal(tarifaFinal, cotizacion.TarifaFinal);
    }

    [Fact]
    public void CalcularTarifa_RedondeaLaTarifaBaseADosDecimales()
    {
        Assert.True(Peso.TryCreate(1.111m, out var peso, out _));

        var cotizacion = _service.CalcularTarifa(peso!, TipoCliente.Nuevo, TipoEnvio.Estandar);

        Assert.Equal(7.22m, cotizacion.TarifaBase);
        Assert.Equal(7.22m, cotizacion.TarifaFinal);
    }

    [Fact]
    public void CalcularTarifa_ConLosMismosDatos_DevuelveElMismoResultado()
    {
        Assert.True(Peso.TryCreate(5.0m, out var peso, out _));

        var primera = _service.CalcularTarifa(peso!, TipoCliente.Frecuente, TipoEnvio.Express);
        var segunda = _service.CalcularTarifa(peso!, TipoCliente.Frecuente, TipoEnvio.Express);

        Assert.Equal(primera.TarifaFinal, segunda.TarifaFinal);
        Assert.Equal(16.88m, primera.TarifaFinal);
    }

    [Fact]
    public void CalcularTarifa_CuandoElPesoEsNulo_LanzaArgumentNullException()
    {
        var excepcion = Assert.Throws<ArgumentNullException>(() =>
            _service.CalcularTarifa(null!, TipoCliente.Nuevo, TipoEnvio.Estandar));

        Assert.Equal("peso", excepcion.ParamName);
    }
}
