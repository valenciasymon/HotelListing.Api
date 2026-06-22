using HotelListing.Api.Constants;
using HotelListing.Api.Contracts;
using HotelListing.Api.Data;
using HotelListing.Api.Data.Enums;
using HotelListing.Api.DTOs.BookingDto;
using HotelListing.Api.Results;
using Microsoft.EntityFrameworkCore;
using System.Collections;
using System.IdentityModel.Tokens.Jwt;

namespace HotelListing.Api.Services;

public class BookingService(
    HotelListingDbContext context,
    IHttpContextAccessor httpContextAccessor) : IBookingService
{
    public async Task<Result<IEnumerable<GetBookingDto>>> GetBookingAsync(int hotelId)
    {
        bool hotelExist = await context.Hotels.AnyAsync(h => h.Id == hotelId);

        if (!hotelExist)
        {
            return Result<IEnumerable<GetBookingDto>>.NotFound(
                new Error(ErrorCodes.NotFound, "Hotel Not Found"));
        }

        List<GetBookingDto> bookings = await context.Bookings
            .Where(b => b.HotelId == hotelId)
            .OrderBy(b => b.CheckIn)
            .Select(b => new GetBookingDto(
                b.BookingId,
                b.HotelId,
                b.Hotel!.Name,
                b.CheckIn,
                b.CheckOut,
                b.Guests,
                b.TotalPrice,
                b.Status.ToString(),
                b.CreatedAtUtc,
                b.UpdateAtUtc)).ToListAsync();

        return Result<IEnumerable<GetBookingDto>>.Success(bookings);
    }

    public async Task<Result<GetBookingDto>> CreateBookingAsync(
        int hotelId,
        CreateBookingDto createBookingDto)
    {
        if (hotelId != createBookingDto.HotelId)
        {
            return Result<GetBookingDto>.BadRequest(new Error(
                ErrorCodes.Validation, "Mismatch Hotel Id"));
        }

        string? userId = httpContextAccessor?
            .HttpContext?
            .User?
            .FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Result<GetBookingDto>.BadRequest(new Error(
                ErrorCodes.Validation, "User is Required"));
        }

        var night = createBookingDto.CheckOut.DayNumber - 
            createBookingDto.CheckIn.DayNumber;

        if (night <= 0)
        {
            return Result<GetBookingDto>.BadRequest(new Error(
                ErrorCodes.Validation, "CheckOut Must Be After CheckIn"));
        }

        if (createBookingDto.Guests <= 0) return Result<GetBookingDto>.BadRequest(
            new Error(ErrorCodes.Validation, "Guest Must Be Atleast 1"));

        Hotel? hotel = await context.Hotels
            .Where(h => h.Id == createBookingDto.HotelId)
            .FirstOrDefaultAsync();

        if (hotel == null) return Result<GetBookingDto>.NotFound(new Error(
            ErrorCodes.NotFound, "Hotel Not Found"));

        bool overlaps = await context.Bookings.AnyAsync(
                b => b.HotelId == createBookingDto.HotelId
                && b.Status != BookingStatus.Cancelled
                && createBookingDto.CheckIn < b.CheckOut
                && createBookingDto.CheckOut > b.CheckIn
                && b.ApplicationUserId == userId);

        if (overlaps)
        {
            return Result<GetBookingDto>.Conflict(
                new Error(
                    ErrorCodes.Conflict,
                    "The Selected Dates Overlap With An Existing Booking"));
        }

        decimal TotalPrice = hotel.PerNightRate * night;

        var booking = new Booking
        {
            HotelId = createBookingDto.HotelId,
            ApplicationUserId = userId,
            CheckIn = createBookingDto.CheckIn,
            CheckOut = createBookingDto.CheckOut,
            Guests = createBookingDto.Guests,
            TotalPrice = TotalPrice,
            Status = BookingStatus.Pending
        };

        await context.AddAsync(booking);
        await context.SaveChangesAsync();

        await context.Entry(booking)
            .Reference(b => b.Hotel)
            .LoadAsync();

        GetBookingDto getBookingDto = new GetBookingDto(
            booking.BookingId,
            booking.HotelId,
            booking.Hotel!.Name,
            booking.CheckIn,
            booking.CheckOut,
            booking.Guests,
            booking.TotalPrice,
            booking.Status.ToString(),
            booking.CreatedAtUtc,
            booking.UpdateAtUtc);

        return Result<GetBookingDto>.Success(getBookingDto);
    }

    public async Task<Result<GetBookingDto>> UpdateBookingAsync(
        int hotelId,int bookingId,UpdateBookingDto updateBookingDto)
    {
        string? userId = httpContextAccessor?
            .HttpContext?
            .User?
            .FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

        if (string.IsNullOrEmpty(userId)) return Result<GetBookingDto>.BadRequest(
            new Error(ErrorCodes.Validation, "User Is Required"));

        int nights = updateBookingDto.CheckOut.DayNumber -
            updateBookingDto.CheckIn.DayNumber;

        if (nights <= 0) return Result<GetBookingDto>.BadRequest(
            new Error(ErrorCodes.Validation, "CheckIn Must Be Before CheckOut"));

        if (updateBookingDto.Guests <= 0) return Result<GetBookingDto>.BadRequest(
            new Error(ErrorCodes.Validation, "Guest Must be Atleast 1"));

        bool overlaps = await context.Bookings.AnyAsync(
            b => b.HotelId == hotelId
            && b.BookingId != bookingId
            && b.Status != BookingStatus.Cancelled
            && updateBookingDto.CheckIn < b.CheckOut
            && updateBookingDto.CheckOut > b.CheckIn
            && b.ApplicationUserId == userId);

        if (overlaps) return Result<GetBookingDto>.Conflict(
            new Error(
                ErrorCodes.Conflict,
                "Selected Dates Overlap With A Existing Booking"));

        Booking? booking = await context.Bookings
            .Include(b => b.Hotel)
            .FirstOrDefaultAsync(b => b.BookingId == bookingId
            && b.HotelId == hotelId
            && b.ApplicationUserId == userId);

        if (booking == null) return Result<GetBookingDto>.NotFound(
            new Error(ErrorCodes.NotFound, "Booking Not Found"));

        if (booking.Status == BookingStatus.Cancelled) return Result<GetBookingDto>.Conflict(
            new Error(ErrorCodes.Conflict, "Cancelled Booking Cannot Be Modified"));

        decimal perNight = booking.Hotel!.PerNightRate;

        booking.CheckIn = updateBookingDto.CheckIn;
        booking.CheckOut = updateBookingDto.CheckOut;
        booking.Guests = updateBookingDto.Guests;
        booking.TotalPrice = perNight * 
            (updateBookingDto.CheckOut.DayNumber - updateBookingDto.CheckIn.DayNumber);
        booking.UpdateAtUtc = DateTime.UtcNow;

        await context.SaveChangesAsync();

        GetBookingDto getBookingDto = new GetBookingDto(
            booking.BookingId,
            booking.HotelId,
            booking.Hotel!.Name,
            booking.CheckIn,
            booking.CheckOut,
            booking.Guests,
            booking.TotalPrice,
            booking.Status.ToString(),
            booking.CreatedAtUtc,
            booking.UpdateAtUtc);

        return Result<GetBookingDto>.Success(getBookingDto);
    }

    public async Task<Result> CanelBookingAsync(int hotelId,int bookingId)
    {
        string? userId = httpContextAccessor?.HttpContext?.User?.FindFirst(
            JwtRegisteredClaimNames.Sub)?.Value;

        if (string.IsNullOrEmpty(userId)) return Result.BadRequest(new Error(
            ErrorCodes.Validation, "User is Required"));

        Booking? booking = await context.Bookings.FirstOrDefaultAsync(
            b => b.BookingId == bookingId
            && b.HotelId == hotelId
            && b.ApplicationUserId == userId);

        if (booking == null) return Result.NotFound(new Error(
            ErrorCodes.NotFound, "Booking Not Found"));

        if (booking.Status == BookingStatus.Cancelled) return Result.Conflict(
            new Error(ErrorCodes.Conflict, "Booking is Already Cancelled"));

        booking.Status = BookingStatus.Cancelled;
        booking.UpdateAtUtc = DateTime.UtcNow;

        await context.SaveChangesAsync();

        return Result.Success();
    }
}
