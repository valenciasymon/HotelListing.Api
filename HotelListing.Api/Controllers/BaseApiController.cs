using HotelListing.Api.Common.Constants;
using HotelListing.Api.Common.Results;
using Microsoft.AspNetCore.Mvc;

namespace HotelListing.Api.Controllers;

public abstract class BaseApiController : ControllerBase
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
            return Problem(
                statusCode:StatusCodes.Status500InternalServerError,
                title: "An Error Occured",
                detail: "No Error Details Provided");
        }

        Error e = errors[0];
        string errorDetails = string.Join("; ", errors.Select(e => e.Description));

        return e.Code switch
        {
            ErrorCodes.NotFound => Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Resource Not Found",
                detail: errorDetails
                ),
            ErrorCodes.Conflict => Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: ErrorCodes.Conflict,
                detail: errorDetails
                ),
            ErrorCodes.Validation => ValidationProblem(
                title: "Validation Failed",
                detail: errorDetails
                ),
            ErrorCodes.BadRequest => Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Badrequest",
                detail: errorDetails
                ),
            ErrorCodes.Forbidden => Problem(
                statusCode: StatusCodes.Status403Forbidden,
                title: ErrorCodes.Forbidden,
                detail: errorDetails),
            _ => Problem(detail: e.Description, title: e.Code)
        };
    }
}
