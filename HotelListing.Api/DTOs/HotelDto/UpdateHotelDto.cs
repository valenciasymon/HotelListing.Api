using System.ComponentModel.DataAnnotations;

namespace HotelListing.Api.DTOs.HotelDto;

public class UpdateHotelDto : CreateHotelDto
{
    [Required]
    public int Id { get; set; }

}