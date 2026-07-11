using HotelListing.Api.DTOs.HotelDto;
using HotelListing.Api.Common.Results;
using HotelListing.Api.Common.Models.Paging;
using HotelListing.Api.Common.Models.Filtering;

namespace HotelListing.Api.Application.Services
{
    public interface IHotelsService
    {
        Task<Result<GetHotelDto>> CreateHotelAsync(CreateHotelDto hotelDto);
        Task<Result> DeleteHotelAsync(int id);
        Task<Result<GetHotelDto>> GetHotelAsync(int id);
        Task<Result<PagedResult<GetHotelsDto>>> GetHotelsAsync(
            PaginationParameters paginationParameters, HotelFilterParameters filters);
        Task<Result> UpdateHotelAsync(int id, UpdateHotelDto hotelDto);
    }
}