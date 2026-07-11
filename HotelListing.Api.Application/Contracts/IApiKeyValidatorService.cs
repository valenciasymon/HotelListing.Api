using HotelListing.Api.Domain;

namespace HotelListing.Api.Application.Contracts
{
    public interface IApiKeyValidatorService
    {
        Task<ApiKey?> IsValidAsync(string apiKey, CancellationToken ct = default);
    }
}