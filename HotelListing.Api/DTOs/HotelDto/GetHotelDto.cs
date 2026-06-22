using HotelListing.Api.Data;

namespace HotelListing.Api.DTOs.HotelDto;

public record GetHotelDto(
    int Id,
    string Name,
    string Address,
    double Rating,
    int? CountryId,
    string? Country);
