using System.Security.Claims;
using CoreApiRest.BusinessRules;
using CoreApiRest.BusinessRules.Interfaces.Controllers.Cotizacion;
using CoreApiRest.BusinessRules.Request.Cotizacion;
using CoreApiRest.BusinessRules.Responses;
using Microsoft.AspNetCore.Mvc;

namespace CoreApiRest.Controllers.Cotizacion;

[ApiController]
[Route("api/v1/quotes")]
public sealed class CotizacionController : ControllerBase
{
    private readonly ICotizacionCalculateController _calculateController;

    public CotizacionController(ICotizacionCalculateController calculateController)
    {
        ArgumentNullException.ThrowIfNull(calculateController);
        _calculateController = calculateController;
    }

    [HttpPost("calculate")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(CalcularCotizacionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Error), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Error), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Calculate([FromBody] CalcularCotizacionRequest request)
    {
        _ = User.FindFirstValue("sub") ?? User.FindFirstValue(ClaimTypes.Actor);

        var result = await _calculateController.Calculate(request);
        if (result.IsSuccess)
            return Ok(result.Value);

        return result.Error!.Type switch
        {
            ErrorType.Conflict => Conflict(result.Error),
            ErrorType.NotFound => NotFound(result.Error),
            ErrorType.Validation => BadRequest(result.Error),
            ErrorType.BusinessRule => UnprocessableEntity(result.Error),
            _ => StatusCode(StatusCodes.Status500InternalServerError, result.Error)
        };
    }
}
