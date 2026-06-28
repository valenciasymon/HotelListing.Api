using System.ComponentModel.DataAnnotations;

namespace HotelListing.Api.DTOs.CountryDto;

public class CreateCountryDto
{
    [Required]
    [MaxLength(20)]
    public string Name { get; set; }
    [Required]
    [MaxLength(15)]
    public string ShortName { get; set; }
}
