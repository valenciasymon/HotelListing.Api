namespace HotelListing.Api.Domain;

public class HotelAdmin
{
    public int HotelAdminId { get; set; }

    public Hotel? Hotel { get; set; }
    public required int HotelId { get; set; }

    public ApplicationUser? ApplicationUser { get; set; }
    public required string ApplicationUserId { get; set; }
}
