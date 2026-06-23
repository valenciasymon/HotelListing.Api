using HotelListing.Api.Contracts;
using HotelListing.Api.DTOs.BookingDto;
using HotelListing.Api.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelListing.Api.Controllers;

[ApiController]
[Route("api/hotels/{hotelId:int}/bookings")]
[Authorize]
public class HotelBookingsController(IBookingService bookingService): BaseApiController
{

    [HttpGet]
    [Authorize(Roles = "Admin,HotelAdmin")]
    public async Task<ActionResult<IEnumerable<GetBookingDto>>> GetBookingAdmin(
        [FromRoute]int hotelId)
    {
        Result<IEnumerable<GetBookingDto>> bookings = await bookingService
            .GetBookingAdminAsync(hotelId);

        return ToActionResult(bookings);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<GetBookingDto>>> GetBooking(
        [FromRoute]int hotelId)
    {
        Result<IEnumerable<GetBookingDto>> bookings = await bookingService
            .GetBookingUserAsync(hotelId);

        return ToActionResult(bookings);
    }

    [HttpPost]
    public async Task<ActionResult<GetBookingDto>> PostBooking(
        [FromRoute] int hotelId,
        CreateBookingDto createBookingDto)
    {
        Result<GetBookingDto> booking = await bookingService
            .CreateBookingAsync(hotelId,createBookingDto);

        return ToActionResult(booking);
    }

    [HttpPut("{bookingId:int}")]
    public async Task<ActionResult<GetBookingDto>> UpdateBooking(
        int hotelId,
        int bookingId,
        UpdateBookingDto updateBookingDto)
    {
        Result<GetBookingDto> booking = await bookingService.UpdateBookingAsync(
            hotelId, bookingId, updateBookingDto);

        return ToActionResult(booking);
    }

    [HttpPut("{bookingId:int}/cancel")]
    public async Task<ActionResult> CancelBooking(int hotelId,int bookingId)
    {
        Result booking = await bookingService.CancelBookingAsync(hotelId, bookingId);

        return ToActionResult(booking);
    }


        [Authorize(Roles ="Admin,HotelAdmin")]
        [HttpPut("{bookingId:int}/admin/cancel")]
        public async Task<ActionResult> AdminCancelBooking(int hotelId,int bookingId)
        {
            Result booking = await bookingService.AdminCancelBookingAsync(hotelId,bookingId);

            return ToActionResult(booking);
        }


        [Authorize(Roles = "Admin,HotelAdmin")]
        [HttpPut("{bookingId:int}/admin/confirm")]
        public async Task<ActionResult> AdminConfirmBooking(int hotelId,int bookingId)
        {
            Result booking = await bookingService.AdminConfirmBookingAsync(hotelId, bookingId);

            return ToActionResult(booking);
        }
}
