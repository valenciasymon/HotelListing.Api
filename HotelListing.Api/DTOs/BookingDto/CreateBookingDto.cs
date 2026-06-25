using System.ComponentModel.DataAnnotations;

namespace HotelListing.Api.DTOs.BookingDto;

public record CreateBookingDto(
    [Required] int HotelId,
    DateOnly CheckIn,
    DateOnly CheckOut,
    [Required][Range(minimum: 1, maximum: 10)] int Guests
) : IValidatableObject
{
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if(CheckOut <= CheckIn)
        {
            yield return new ValidationResult(
                "Checkout Must Be After CheckIn",
                ["CheckOut", "Checkin"]);
        }
    }
}