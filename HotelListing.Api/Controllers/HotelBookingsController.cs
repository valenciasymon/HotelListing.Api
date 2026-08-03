using HotelListing.Api.AuthorizationFilters;
using HotelListing.Api.Common.Constants;
using HotelListing.Api.Common.Results;
using HotelListing.Api.Application.Contracts;
using HotelListing.Api.DTOs.BookingDto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HotelListing.Api.Common.Models.Paging;
using HotelListing.Api.Common.Models.Filtering;
using Microsoft.AspNetCore.RateLimiting;

namespace HotelListing.Api.Controllers;

[ApiController]
[Route("api/hotels/{hotelId:int}/bookings")]
[Authorize]
[EnableRateLimiting("perUser")]
public class HotelBookingsController(IBookingService bookingService) : BaseApiController
{

    [HttpGet(RoleNames.Admin)]
    [HotelSystemAdmin]
    public async Task<ActionResult<PagedResult<GetBookingDto>>> GetBookingAdmin(
        [FromRoute] int hotelId,
        [FromQuery] PaginationParameters paginationParameters,
        [FromQuery] BookingFilterParameters filters)
    {
        Result<PagedResult<GetBookingDto>> bookings = await bookingService
            .GetBookingAdminAsync(hotelId,paginationParameters,filters);

        return ToActionResult(bookings);
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<GetBookingDto>>> GetBooking(
        [FromRoute] int hotelId,
        [FromQuery] PaginationParameters paginationParameters,
        [FromQuery] BookingFilterParameters filters)
    {
        Result<PagedResult<GetBookingDto>> bookings = await bookingService
            .GetBookingUserAsync(hotelId,paginationParameters,filters);

        return ToActionResult(bookings);
    }

    [HttpPost]
    public async Task<ActionResult<GetBookingDto>> PostBooking(
        [FromRoute] int hotelId,
        CreateBookingDto createBookingDto)
    {
        Result<GetBookingDto> booking = await bookingService
            .CreateBookingAsync(hotelId, createBookingDto);

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
    public async Task<ActionResult> CancelBooking(int hotelId, int bookingId)
    {
        Result booking = await bookingService.CancelBookingAsync(hotelId, bookingId);

        return ToActionResult(booking);
    }


    [HotelSystemAdmin]
    [HttpPut("{bookingId:int}/admin/cancel")]
    public async Task<ActionResult> AdminCancelBooking(int hotelId, int bookingId)
    {
        Result booking = await bookingService.AdminCancelBookingAsync(hotelId, bookingId);

        return ToActionResult(booking);
    }


    [HotelSystemAdmin]
    [HttpPut("{bookingId:int}/admin/confirm")]
    public async Task<ActionResult> AdminConfirmBooking(int hotelId, int bookingId)
    {
        Result booking = await bookingService.AdminConfirmBookingAsync(hotelId, bookingId);

        return ToActionResult(booking);
    }
}
