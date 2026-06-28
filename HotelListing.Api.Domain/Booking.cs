using HotelListing.Api.Domain.Enums;

namespace HotelListing.Api.Domain;

public class Booking
{
    public int BookingId { get; set; }

    public required int HotelId { get; set; }
    public Hotel? Hotel { get; set; }

    public required string ApplicationUserId { get; set; } 
    public ApplicationUser? ApplicationUser { get; set; }

    public DateOnly CheckIn { get; set; }
    public DateOnly CheckOut { get; set; }
    public int Guests { get; set; }

    public decimal TotalPrice { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdateAtUtc { get; set; }

    public BookingStatus Status { get; set; } = BookingStatus.Pending;
}