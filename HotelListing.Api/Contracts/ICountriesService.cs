using HotelListing.Api.DTOs.CountryDto;
using HotelListing.Api.Results;

namespace HotelListing.Api.Services
{
    public interface ICountriesService
    {
        Task<Result<GetCountryDto>> CreateCountryAsync(CreateCountryDto countryDto);
        Task<Result> DeleteCountryAsync(int id);
        Task<Result<IEnumerable<GetCountriesDto>>> GetCountriesAsync();
        Task<Result<GetCountryDto>> GetCountryAsync(int id);
        Task<Result> UpdateCountryAsync(int id, UpdateCountryDto countryDto);
    }
}