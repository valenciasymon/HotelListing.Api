using HotelListing.Api.DTOs.BookingDto;
using HotelListing.Api.Results;

namespace HotelListing.Api.Contracts
{
    public interface IBookingService
    {
        Task<Result<IEnumerable<GetBookingDto>>> GetBookingAsync(int hotelId);
        Task<Result<GetBookingDto>> CreateBookingAsync(CreateBookingDto createBookingDto);
        Task<Result<GetBookingDto>> UpdateBookingAsync(int hotelId, int bookingId, UpdateBookingDto updateBookingDto);
        Task<Result> CancelBookingAsync(int hotelId, int bookingId);
    }
}