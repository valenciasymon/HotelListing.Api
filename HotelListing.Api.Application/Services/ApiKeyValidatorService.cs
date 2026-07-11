using HotelListing.Api.Application.Contracts;
using HotelListing.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace HotelListing.Api.Application.Services;

public class ApiKeyValidatorService(HotelListingDbContext context) : IApiKeyValidatorService
{
    public async Task<ApiKey?> IsValidAsync(string apiKey, CancellationToken ct = default)
    {
        ApiKey? key = await context.ApiKeys
            .AsNoTracking()
            .FirstOrDefaultAsync(k => k.KeyValue  == apiKey,ct);

        if (key == null) return null;

        return key;
    }

}
