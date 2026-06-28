namespace HotelListing.Api.DTOs.BookingDto;

public record GetBookingDto(
    int BookingId,
    int HotelId,
    string HotelName,
    DateOnly CheckIn,
    DateOnly CheckOut,
    int Guests,
    decimal TotalPrice,
    string Status,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);
