using Microsoft.AspNetCore.Mvc;
using HotelListing.Api.DTOs.CountryDto;
using HotelListing.Api.Services;
using HotelListing.Api.Results;
using HotelListing.Api.Controllers;
using Microsoft.AspNetCore.Authorization;


[Route("api/[controller]")]
[ApiController]
[Authorize]
public class CountriesController(ICountriesService countriesService) : BaseApiController
{

    // GET: api/Country
    [HttpGet]
    public async Task<ActionResult<IEnumerable<GetCountriesDto>>> GetCountry()
    {
        Result<IEnumerable<GetCountriesDto>> result = await countriesService.GetCountriesAsync();

        return ToActionResult<IEnumerable<GetCountriesDto>>(result);
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
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> PutCountry(int id, UpdateCountryDto countryDto)
    {

        Result country = await countriesService.UpdateCountryAsync(id,countryDto);

        return ToActionResult(country);
    }

    // POST: api/Country
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    [Authorize(Roles = "Admin")]
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
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> DeleteCountry(int id)
    {
        Result country = await countriesService.DeleteCountryAsync(id);

        return ToActionResult(country);
        
    }

}
