namespace HotelListing.Api.DTOs.HotelDto;

public record GetHotelsDto(
    int Id,
    string Name,
    string Address,
    double Rating);
