using HotelListing.Api.Common.Constants;
using HotelListing.Api.Common.Results;
using HotelListing.Api.Controllers;
using HotelListing.Api.DTOs.HotelDto;
using HotelListing.Api.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class HotelsController(IHotelsService hotelsService) : BaseApiController
{

    // GET: api/Hotel
    [HttpGet]
    public async Task<ActionResult<IEnumerable<GetHotelsDto>>> GetHotel()
    {
        Result<IEnumerable<GetHotelsDto>> hotels = await hotelsService.GetHotelsAsync();

        return ToActionResult(hotels);
    }

    // GET: api/Hotel/5
    [HttpGet("{id}")]
    public async Task<ActionResult<GetHotelDto>> GetHotel(int id)
    {
        Result<GetHotelDto> hotel = await hotelsService.GetHotelAsync(id);

        return ToActionResult(hotel);
    }

    // PUT: api/Hotel/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{id}")]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<IActionResult> PutHotel(int id, UpdateHotelDto hotelDto)
    {
        Result hotel = await hotelsService.UpdateHotelAsync(id,hotelDto);

        return ToActionResult(hotel);
    }

    // POST: api/Hotel
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<ActionResult<GetHotelDto>> PostHotel(CreateHotelDto hotelDto)
    {
        Result<GetHotelDto> hotel = await hotelsService.CreateHotelAsync(hotelDto);

        if (!hotel.IsSuccess)
        {
            return ToActionResult(hotel);
        }

        return CreatedAtAction("GetHotel", new { id = hotel.Value!.Id }, hotel.Value);
    }

    // DELETE: api/Hotel/5
    [HttpDelete("{id}")]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<IActionResult> DeleteHotel(int id)
    {
        Result hotel = await hotelsService.DeleteHotelAsync(id);

        return ToActionResult(hotel);
    }

   
}
