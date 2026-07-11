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


[Route("api/[controller]")]
[ApiController]
[Authorize]
public class CountriesController(ICountriesService countriesService) : BaseApiController
{

    // GET: api/Country
    [HttpGet]
    public async Task<ActionResult<IEnumerable<GetCountriesDto>>> GetCountries(
        [FromQuery]CountriesFilterParameter filters)
    {
        Result<IEnumerable<GetCountriesDto>> result = await countriesService
            .GetCountriesAsync(filters);

        return ToActionResult<IEnumerable<GetCountriesDto>>(result);
    }

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

    // GET: api/Country/5
    [HttpGet("{id}")]
    public async Task<ActionResult<GetCountryDto>> GetCountry(int id)
    {
        Result<GetCountryDto> country = await countriesService.GetCountryAsync(id);


        return ToActionResult<GetCountryDto>(country);
    }

    // PUT: api/Country/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{id}")]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<ActionResult> PutCountry(int id, UpdateCountryDto countryDto)
    {

        Result country = await countriesService.UpdateCountryAsync(id,countryDto);

        return ToActionResult(country);
    }

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

    // POST: api/Country
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
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

    // DELETE: api/Country/5
    [HttpDelete("{id}")]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<ActionResult> DeleteCountry(int id)
    {
        Result country = await countriesService.DeleteCountryAsync(id);

        return ToActionResult(country);
        
    }

}
