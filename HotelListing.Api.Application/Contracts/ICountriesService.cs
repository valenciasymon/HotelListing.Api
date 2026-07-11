
using HotelListing.Api.Common.Models.Filtering;
using HotelListing.Api.Common.Models.Paging;
using HotelListing.Api.Common.Results;
using HotelListing.Api.DTOs.CountryDto;
using HotelListing.Api.DTOs.HotelDto;
using Microsoft.AspNetCore.JsonPatch;

namespace HotelListing.Api.Application.Services
{
    public interface ICountriesService
    {
        Task<Result<GetCountryDto>> CreateCountryAsync(CreateCountryDto countryDto);
        Task<Result> DeleteCountryAsync(int id);
        Task<Result<IEnumerable<GetCountriesDto>>> GetCountriesAsync(
            CountriesFilterParameter filters);
        Task<Result<GetCountryDto>> GetCountryAsync(int id);
        Task<Result> UpdateCountryAsync(int id, UpdateCountryDto countryDto);
        Task<Result<PagedResult<GetHotelDto>>> GetCountriesHotelAsync(
            int countryId,
            PaginationParameters paginationParameters,
            HotelFilterParameters filters);
        Task<Result> PatchCountryAsync(int id, JsonPatchDocument<UpdateCountryDto> patchDoc);
    }
}