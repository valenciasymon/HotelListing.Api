using HotelListing.Api.DTOs.BookingDto;
using HotelListing.Api.Common.Results;
using HotelListing.Api.Common.Models.Paging;
using HotelListing.Api.Common.Models.Filtering;

namespace HotelListing.Api.Application.Contracts
{
    public interface IBookingService
    {
        Task<Result<PagedResult<GetBookingDto>>> GetBookingAdminAsync(
            int hotelId,
            PaginationParameters paginationParameters,
            BookingFilterParameters filters);
        Task<Result<GetBookingDto>> CreateBookingAsync(int hotelId,CreateBookingDto createBookingDto);
        Task<Result<GetBookingDto>> UpdateBookingAsync(int hotelId, int bookingId, UpdateBookingDto updateBookingDto);
        Task<Result> CancelBookingAsync(int hotelId, int bookingId);
        Task<Result> AdminCancelBookingAsync(int hotelId, int bookingId);
        Task<Result> AdminConfirmBookingAsync(int hotelId, int bookingId);
        Task<Result<PagedResult<GetBookingDto>>> GetBookingUserAsync(
            int hotelId,
            PaginationParameters paginationParameters,
            BookingFilterParameters filters);
    }
}