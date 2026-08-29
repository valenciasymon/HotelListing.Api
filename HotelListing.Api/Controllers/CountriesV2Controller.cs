using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace HotelListing.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/countries")]
[ApiVersion("2.0",Deprecated = true)]
public class CountriesV2Controller : BaseApiController
{
    [HttpGet]
    public IActionResult GetCountries(
        [FromQuery] int? pageNumber = 1,
        [FromQuery] int? pageSize = 10)
    {
        return Ok(new
        {
            Version = "2.0",
            Message = "Enchanced Countries endpoint with pagination",
            PageNumber = pageNumber,
            PageSize = pageSize
        });
    }
}
