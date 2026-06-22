using HotelListing.Api.Contracts;
using HotelListing.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace HotelListing.Api.Services;

public class ApiKeyValidatorService(HotelListingDbContext context) : IApiKeyValidatorService
{
    public async Task<bool> IsValidAsync(string apiKey, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return false;
        }

       ApiKey? key = await context.ApiKeys
            .AsNoTracking()
            .FirstOrDefaultAsync(k => k.KeyValue  == apiKey,ct);

        if (key == null) return false;

        return key.IsActive;
    }

}
