using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace HotelListing.Api.DTOs.HotelDto;

public class CreateHotelDto
{
    [Required]
    public string Name { get; set; }
    [Required]
    [MaxLength(30)]
    public string Address { get; set; }
    [Required]
    [Range(1,5)]
    public double Rating { get; set; }
    [Required]
    public decimal PerNightRate { get; set; }
    public int? CountryId { get; set; }
}
