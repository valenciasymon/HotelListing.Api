using HotelListing.Api.Contracts;
using HotelListing.Api.Data;
using HotelListing.Api.DTOs.AuthDto;
using HotelListing.Api.Results;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;

namespace HotelListing.Api.Handlers
{
    public class BasicAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IUserService userService
        ) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
    {
        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.TryGetValue(
                "Authorization",
                out StringValues authHeaderValues))
            {
                return AuthenticateResult.NoResult();
            }

            string authHeader = authHeaderValues.ToString();
            if (string.IsNullOrWhiteSpace(authHeader) || !authHeader.StartsWith("Basic ",
                StringComparison.OrdinalIgnoreCase))
            {
                return AuthenticateResult.NoResult();
            }

            string token = authHeader["Basic ".Length..].Trim();

            string decoded;

            try
            {
                byte[] credentialsBytes = Convert.FromBase64String(token);
                decoded = Encoding.UTF8.GetString(credentialsBytes);
            }
            catch
            {
                return AuthenticateResult.Fail("Invalid Basic authentication token");
            }

            int separatorIndex = decoded.IndexOf(":");
            if (separatorIndex <= 0)
            {
                return AuthenticateResult.Fail("Invalid Basic Authentication Credentials Format");
            }

            string userNameOrEmail = decoded[..separatorIndex];
            string password = decoded[(separatorIndex + 1)..];

            LoginUserDto loginUserDto = new LoginUserDto
            {
                Email = userNameOrEmail,
                Password = password
            };

            Result<string> result =  await userService.LoginAsync(loginUserDto);

            if (!result.IsSuccess)
            {
                return AuthenticateResult.Fail("Invalid Username or Password");
            }

            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name,userNameOrEmail)
            };

            ClaimsIdentity identity = new ClaimsIdentity(claims,Scheme.Name);
            ClaimsPrincipal principal = new ClaimsPrincipal(identity);
            AuthenticationTicket ticket = new AuthenticationTicket(principal,Scheme.Name);


            return AuthenticateResult.Success(ticket);
        }
    }
}
