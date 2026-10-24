using CoreApiRest.BusinessRules;

namespace CoreApiRest.Tests;

public class TarifaCalculatorServiceTests
{
    private readonly TarifaCalculatorService _service = new();

    public static TheoryData<decimal, TipoCliente, TipoEnvio, decimal, decimal, decimal, decimal> Combinaciones => new()
    {
        { 5.0m, TipoCliente.Regular, TipoEnvio.Estandar, 25.00m, 1.00m, 1.00m, 25.00m },
        { 5.0m, TipoCliente.Regular, TipoEnvio.Express, 25.00m, 1.00m, 1.50m, 37.50m },
        { 5.0m, TipoCliente.Corporativo, TipoEnvio.Estandar, 25.00m, 0.90m, 1.00m, 22.50m },
        { 5.0m, TipoCliente.Corporativo, TipoEnvio.Express, 25.00m, 0.90m, 1.50m, 33.75m },
        { 5.0m, TipoCliente.VIP, TipoEnvio.Estandar, 25.00m, 0.80m, 1.00m, 20.00m },
        { 5.0m, TipoCliente.VIP, TipoEnvio.Express, 25.00m, 0.80m, 1.50m, 30.00m },
        { 0.01m, TipoCliente.Regular, TipoEnvio.Estandar, 0.05m, 1.00m, 1.00m, 0.05m },
        { 20.0m, TipoCliente.VIP, TipoEnvio.Express, 100.00m, 0.80m, 1.50m, 120.00m }
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

        var cotizacion = _service.CalcularTarifa(peso!, TipoCliente.Regular, TipoEnvio.Estandar);

        Assert.Equal(5.56m, cotizacion.TarifaBase);
        Assert.Equal(5.56m, cotizacion.TarifaFinal);
    }

    [Fact]
    public void CalcularTarifa_ConLosMismosDatos_DevuelveElMismoResultado()
    {
        Assert.True(Peso.TryCreate(5.0m, out var peso, out _));

        var primera = _service.CalcularTarifa(peso!, TipoCliente.Corporativo, TipoEnvio.Express);
        var segunda = _service.CalcularTarifa(peso!, TipoCliente.Corporativo, TipoEnvio.Express);

        Assert.Equal(primera.TarifaFinal, segunda.TarifaFinal);
        Assert.Equal(33.75m, primera.TarifaFinal);
    }

    [Fact]
    public void CalcularTarifa_CuandoElPesoEsNulo_LanzaArgumentNullException()
    {
        var excepcion = Assert.Throws<ArgumentNullException>(() =>
            _service.CalcularTarifa(null!, TipoCliente.Regular, TipoEnvio.Estandar));

        Assert.Equal("peso", excepcion.ParamName);
    }
}
