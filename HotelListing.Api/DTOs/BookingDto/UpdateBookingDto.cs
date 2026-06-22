using Microsoft.VisualBasic;

namespace HotelListing.Api.DTOs.BookingDto
{
    public class UpdateBookingDto
    {
        public required DateOnly CheckIn { get; set; }
        public  required DateOnly CheckOut { get; set; }
        public required int Guests { get; set; }
    }
}
