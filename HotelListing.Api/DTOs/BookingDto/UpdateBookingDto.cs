using System.ComponentModel.DataAnnotations;

namespace HotelListing.Api.DTOs.BookingDto
{
    public class UpdateBookingDto : IValidatableObject
    {
        public required DateOnly CheckIn { get; set; }
        public required DateOnly CheckOut { get; set; }
        [Required]
        [Range(minimum: 1, maximum: 10)]
        public required int Guests { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (CheckOut <= CheckIn)
            {
                yield return new ValidationResult(
                    "Checkout Must Be After CheckIn",
                    ["CheckOut", "Checkin"]);
            }
        }
    }

}
