using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelListing.Api.Domain.Configurations;

public class HotelsConfiguration : IEntityTypeConfiguration<Hotel>
{
    public void Configure(EntityTypeBuilder<Hotel> builder)
    {
        builder.HasIndex(h => h.Name);
        builder.HasIndex(h => h.CountryId);
        builder.HasIndex(h => new { h.CountryId, h.Rating });
    }
}