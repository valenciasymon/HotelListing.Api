namespace HotelListing.Api.DTOs.BookingDto;

public record CreateBookingDto(
    int HotelId,
    DateOnly CheckIn,
    DateOnly CheckOut,
    int Guests);
