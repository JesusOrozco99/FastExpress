using CoreApiRest.BusinessRules;

namespace CoreApiRest.Tests;

public class PesoTests
{
    public static TheoryData<decimal, bool> ValoresLimite => new()
    {
        { 0m, false },
        { 0.01m, true },
        { 5.0m, true },
        { 20.0m, true },
        { 20.01m, false }
    };

    [Theory]
    [MemberData(nameof(ValoresLimite))]
    public void TryCreate_AnalisisDeValoresLimite_AceptaSoloElRangoDeBr02(decimal pesoKg, bool esperadoValido)
    {
        var creado = Peso.TryCreate(pesoKg, out var peso, out var error);

        Assert.Equal(esperadoValido, creado);
        if (esperadoValido)
        {
            Assert.NotNull(peso);
            Assert.Equal(pesoKg, peso.Valor);
            Assert.Null(error);
        }
        else
        {
            Assert.Null(peso);
            Assert.NotNull(error);
        }
    }

    [Fact]
    public void TryCreate_CuandoElPesoEsCero_ExplicaElRangoPermitido()
    {
        var creado = Peso.TryCreate(0m, out _, out var error);

        Assert.False(creado);
        Assert.Equal(
            "El peso debe ser mayor a 0.00 kg y menor o igual a 20.00 kg. Valor recibido: 0.00 kg.",
            error);
    }

    [Fact]
    public void TryCreate_CuandoElPesoSuperaVeinte_ExplicaElRangoPermitido()
    {
        var creado = Peso.TryCreate(20.01m, out _, out var error);

        Assert.False(creado);
        Assert.Equal(
            "El peso debe ser mayor a 0.00 kg y menor o igual a 20.00 kg. Valor recibido: 20.01 kg.",
            error);
    }

    [Fact]
    public void TryCreate_CuandoElPesoEsNegativo_RechazaElValor()
    {
        var creado = Peso.TryCreate(-0.01m, out var peso, out var error);

        Assert.False(creado);
        Assert.Null(peso);
        Assert.Contains("Valor recibido: -0.01 kg.", error, StringComparison.Ordinal);
    }
}
