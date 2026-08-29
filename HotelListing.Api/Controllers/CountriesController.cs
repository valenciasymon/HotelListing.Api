using Microsoft.AspNetCore.Mvc;
using HotelListing.Api.DTOs.CountryDto;
using HotelListing.Api.Application.Services;
using HotelListing.Api.Common.Results;
using HotelListing.Api.Controllers;
using Microsoft.AspNetCore.Authorization;
using HotelListing.Api.Common.Constants;
using HotelListing.Api.DTOs.HotelDto;
using HotelListing.Api.Common.Models.Paging;
using HotelListing.Api.Common.Models.Filtering;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.AspNetCore.RateLimiting;
using Asp.Versioning;

[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
[ApiVersion("1.0")]
[Authorize]
[EnableRateLimiting("fixed")]
public class CountriesController(ICountriesService countriesService) : BaseApiController
{
    /// <summary>
    /// Retrieves a list of countries using the specified filtering criteria.
    /// </summary>
    /// <param name="filters">The filtering parameters used to search for countries.</param>
    /// <returns>A collection of countries matching the specified filters.</returns>
    /// <response code="200">Countries were successfully retrieved.</response>
    // GET: api/Country
    [HttpGet]
    [OutputCache(PolicyName = CacheConstants.AuthenticatedUserCachingPolicy)]
    public async Task<ActionResult<IEnumerable<GetCountriesDto>>> GetCountries(
        [FromQuery]CountriesFilterParameter filters)
    {
        Result<IEnumerable<GetCountriesDto>> result = await countriesService
            .GetCountriesAsync(filters);

        return ToActionResult<IEnumerable<GetCountriesDto>>(result);
    }

    /// <summary>
    /// Retrieves the hotels belonging to a specific country using filtering, sorting, and pagination.
    /// </summary>
    /// <param name="countryId">The unique identifier of the country.</param>
    /// <param name="paginationParameters">The pagination settings used to determine the page and page size.</param>
    /// <param name="filters">The filtering and sorting parameters used to filter the hotels.</param>
    /// <returns>A paginated collection of hotels belonging to the specified country.</returns>
    /// <response code="200">Hotels were successfully retrieved.</response>
    /// <response code="404">The specified country was not found.</response>
    [HttpGet("{countryId:int}/hotels")]
    public async Task<ActionResult<PagedResult<GetHotelDto>>> GetCountriesHotel(
        [FromRoute] int countryId,
        [FromQuery] PaginationParameters paginationParameters,
        [FromQuery] HotelFilterParameters filters)
    {
        Result<PagedResult<GetHotelDto>> hotels = await countriesService.GetCountriesHotelAsync(
            countryId,
            paginationParameters,
            filters);

        return ToActionResult(hotels);
    }

    /// <summary>
    /// Retrieves a country by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the country.</param>
    /// <returns>The country matching the specified identifier.</returns>
    /// <response code="200">The country was successfully retrieved.</response>
    /// <response code="404">The specified country was not found.</response>
    // GET: api/Country/5
    [HttpGet("{id}")]
    public async Task<ActionResult<GetCountryDto>> GetCountry(int id)
    {
        Result<GetCountryDto> country = await countriesService.GetCountryAsync(id);


        return ToActionResult<GetCountryDto>(country);
    }


    /// <summary>
    /// Updates an existing country.
    /// </summary>
    /// <param name="id">The unique identifier of the country to update.</param>
    /// <param name="countryDto">The country data used to update the existing country.</param>
    /// <returns>The result of the update operation.</returns>
    /// <response code="204">The country was successfully updated.</response>
    /// <response code="400">The route ID does not match the payload ID.</response>
    /// <response code="404">The specified country was not found.</response>
    /// <response code="409">A country with the specified name already exists.</response>
    /// <response code="500">An unexpected server error occurred.</response>
    // PUT: api/Country/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{id}")]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<ActionResult> PutCountry(int id, UpdateCountryDto countryDto)
    {

        Result country = await countriesService.UpdateCountryAsync(id,countryDto);

        return ToActionResult(country);
    }

    /// <summary>
    /// Partially updates an existing country using a JSON Patch document.
    /// </summary>
    /// <param name="id">The unique identifier of the country to update.</param>
    /// <param name="patchDoc">The JSON Patch document containing the operations to apply.</param>
    /// <returns>The result of the patch operation.</returns>
    /// <response code="204">The country was successfully updated.</response>
    /// <response code="400">The patch document is invalid or attempts to modify the country ID.</response>
    /// <response code="404">The specified country was not found.</response>
    /// <response code="409">A country with the same name already exists.</response>
    [HttpPatch("{id:int}")]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<ActionResult> PatchCountry(
        int id,
        [FromBody]JsonPatchDocument<UpdateCountryDto> patchDoc)
    {
        if (patchDoc == null)
            return BadRequest("Patch Document is Required");

        if (patchDoc.Operations.Count == 0)
            return BadRequest("Patch Must Contain Atleast One Operation");

        Result country = await countriesService.PatchCountryAsync(id, patchDoc);

        return ToActionResult(country);
    }

    /// <summary>
    /// Creates a new country.
    /// </summary>
    /// <param name="countryDto">The data used to create the country.</param>
    /// <returns>The newly created country.</returns>
    /// <response code="201">The country was successfully created.</response>
    /// <response code="409">A country with the specified name already exists.</response>
    /// <response code="500">An unexpected server error occurred.</response>
    // POST: api/Country
    [HttpPost]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<ActionResult<GetCountryDto>> PostCountry(CreateCountryDto countryDto)
    {
        Result<GetCountryDto> country = await countriesService
            .CreateCountryAsync(countryDto);

        if (!country.IsSuccess)
        {
            return ToActionResult<GetCountryDto>(country);
        }

        return CreatedAtAction(
            "GetCountry",
            new { id = country.Value!.Id }, 
            country.Value);
    }

    ///<summary>
    /// Deletes a country by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the country to delete.</param>
    /// <returns>The result of the delete operation.</returns>
    /// <response code="204">The country was successfully deleted.</response>
    /// <response code="404">The specified country was not found.</response>
    /// <response code="500">An unexpected server error occurred.</response>
    // DELETE: api/Country/5
    [HttpDelete("{id}")]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<ActionResult> DeleteCountry(int id)
    {
        Result country = await countriesService.DeleteCountryAsync(id);

        return ToActionResult(country);
        
    }

}
