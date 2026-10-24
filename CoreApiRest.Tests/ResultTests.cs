using CoreApiRest.BusinessRules;

namespace CoreApiRest.Tests;

public class ResultTests
{
    [Fact]
    public void Success_ConservaElValorYNoTieneError()
    {
        var result = Result.Success("cotizado");

        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Equal("cotizado", result.Value);
        Assert.Null(result.Error);
    }

    [Fact]
    public void Failure_ConservaElErrorYNoTieneValor()
    {
        var error = new Error("BR-02", "peso inválido", ErrorType.BusinessRule);

        var result = Result.Failure<string>(error);
        var actual = result.Error;

        Assert.True(result.IsFailure);
        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.Same(error, actual);
        Assert.NotNull(actual);
        Assert.Equal("BR-02", actual.Code);
        Assert.Equal("peso inválido", actual.Message);
        Assert.Equal(ErrorType.BusinessRule, actual.Type);
    }

    [Fact]
    public void Success_CuandoElValorEsNulo_LanzaArgumentNullException()
    {
        var excepcion = Assert.Throws<ArgumentNullException>(() => Result<string>.Success(null!));

        Assert.Equal("value", excepcion.ParamName);
    }

    [Fact]
    public void Failure_CuandoElErrorEsNulo_LanzaArgumentNullException()
    {
        var excepcion = Assert.Throws<ArgumentNullException>(() => Result<string>.Failure(null!));

        Assert.Equal("error", excepcion.ParamName);
    }
}
