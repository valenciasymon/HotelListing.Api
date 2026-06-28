namespace HotelListing.Api.DTOs.HotelDto;

public record GetHotelDto(
    int Id,
    string Name,
    string Address,
    double Rating,
    decimal PerNightRate,
    int? CountryId,
    string? Country);
