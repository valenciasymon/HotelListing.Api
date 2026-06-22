using HotelListing.Api.Constants;
using HotelListing.Api.Results;
using Microsoft.AspNetCore.Mvc;
using System.Reflection.Metadata.Ecma335;

namespace HotelListing.Api.Controllers;

public abstract class BaseApiController :ControllerBase
{
    protected ActionResult<T> ToActionResult<T>(Result<T> result)
    {
        return result.IsSuccess 
            ? Ok(result.Value) 
            : MapErrorToActionResult(result.Errors);
    }
    protected ActionResult ToActionResult(Result result)
    {
        return result.IsSuccess
            ? NoContent()
            : MapErrorToActionResult(result.Errors);
    }

    protected ActionResult MapErrorToActionResult(Error[] errors)
    {
        if (errors.Length == 0)
        {
            return Problem();
        }

        Error e = errors[0];

        return e.Code switch
        {
            ErrorCodes.NotFound => NotFound(e.Description),
            ErrorCodes.Conflict => Conflict(e.Description),
            ErrorCodes.Validation => BadRequest(e.Description),
            ErrorCodes.BadRequest => BadRequest(e.Description),
            ErrorCodes.Forbidden => Forbid(e.Description),
            _ => Problem(detail: e.Description,title: e.Code)
        }; 
    }
}
