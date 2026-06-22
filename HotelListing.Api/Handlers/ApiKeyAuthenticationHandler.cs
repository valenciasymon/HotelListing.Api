using HotelListing.Api.Constants;
using HotelListing.Api.Contracts;
using HotelListing.Api.DTOs.AuthDto;
using HotelListing.Api.Results;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;

namespace HotelListing.Api.Handlers
{
    public class ApiKeyAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IApiKeyValidatorService apiKeyValidatorService
        ) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
    {
        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            string apiKey = string.Empty;

            if (Request.Headers.TryGetValue(
                AuthenticationDefaults.ApiKeyHeaderName,
                out StringValues headerValues))
            {
                apiKey = headerValues.ToString();
            }

            if (string.IsNullOrEmpty(apiKey))
            {
                return AuthenticateResult.NoResult();
            }

            var valid = await apiKeyValidatorService.IsValidAsync(
                apiKey,
                Context.RequestAborted);

            if (!valid)
            {
                return AuthenticateResult.Fail("Invalid API Key");
            }

            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier,"apikey"),
                new Claim(ClaimTypes.Name,"ApiKeyClient")
            };

            ClaimsIdentity identity = new ClaimsIdentity(claims, Scheme.Name);
            ClaimsPrincipal principal = new ClaimsPrincipal(identity);
            AuthenticationTicket ticket = new AuthenticationTicket(principal, Scheme.Name);


            return AuthenticateResult.Success(ticket);
        }
    }
}
