using HotelListing.Api.Application.Contracts;
using HotelListing.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace HotelListing.Api.Application.Services;

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
