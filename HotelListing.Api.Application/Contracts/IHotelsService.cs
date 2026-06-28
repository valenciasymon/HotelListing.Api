using HotelListing.Api.DTOs.HotelDto;
using HotelListing.Api.Common.Results;

namespace HotelListing.Api.Application.Services
{
    public interface IHotelsService
    {
        Task<Result<GetHotelDto>> CreateHotelAsync(CreateHotelDto hotelDto);
        Task<Result> DeleteHotelAsync(int id);
        Task<Result<GetHotelDto>> GetHotelAsync(int id);
        Task<Result<IEnumerable<GetHotelsDto>>> GetHotelsAsync();
        Task<Result> UpdateHotelAsync(int id, UpdateHotelDto hotelDto);
    }
}