using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelListing.Api.Data.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<IdentityRole>
{
    public void Configure(EntityTypeBuilder<IdentityRole> builder)
    {
        builder.HasData(
            new IdentityRole
            {
                Id = "c1c8d006-3fc7-425d-be50-6742be8c16ff",
                Name = "Admin",
                NormalizedName = "ADMIN",
                ConcurrencyStamp = "6b1c4971-54bc-4f0b-9565-89fe04466c20"
            },
            new IdentityRole
            {
                Id = "9ecb7124-41f1-4452-b7d9-621f841d34bf",
                Name = "User",
                NormalizedName = "USER",
                ConcurrencyStamp = "0dfcacd1-37d1-4672-b10c-dc6559b733c9"
            },
            new IdentityRole
            {
                Id = "411571d5-9a71-4667-a8ad-592df9cbef6b",
                Name = "HotelAdmin",
                NormalizedName = "HOTELADMIN",
                ConcurrencyStamp = "598beec0-1d74-4d50-a2e7-0447f5fc8fbf"
            });
    }
}
