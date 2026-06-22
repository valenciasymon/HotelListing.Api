using HotelListing.Api.Data;
using HotelListing.Api.DTOs.HotelDto;
namespace HotelListing.Api.DTOs.CountryDto;
public record GetCountryDto(
    int Id,
    string Name,
    string ShortName,
    IList<CountryHotelsDto> Hotels);

