using Microsoft.AspNetCore.Mvc;
using Sefic.Partners.Services.Helper;
using Sefic.Partners.Shared.Models;

namespace Sefic.Partners.Api.Helper;

[ApiController]
public abstract class BaseController : ControllerBase
{
    protected IActionResult ProcessResult<T>(ServiceResult<T> result)
    {
        return result.StatusCode switch
        {
            200 => Ok(result.Data),
            201 => result.Data != null ? Ok(result.Data) : StatusCode(201),
            400 => BadRequest(new ErrorResponse(400, result.Message, result.Errors)),
            404 => NotFound(new ErrorResponse(404, result.Message)),
            409 => Conflict(new ErrorResponse(409, result.Message)),
            _ => StatusCode(result.StatusCode, new ErrorResponse(result.StatusCode, result.Message))
        };
    }
}
