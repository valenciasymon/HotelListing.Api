using HotelListing.Api.Common.Constants;
using HotelListing.Api.Common.Results;
using HotelListing.Api.Application.Contracts;
using HotelListing.Api.Domain;
using HotelListing.Api.DTOs.AuthDto;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using HotelListing.Api.Common.Models.Config;

namespace HotelListing.Api.Application.Services;

public class UserService(
    UserManager<ApplicationUser> userManager,
    IOptions<JwtSettings> options,
    RoleManager<IdentityRole> roleManager,
    HotelListingDbContext context) : IUserService
{
    public async Task<Result<RegisteredUserDto>> RegisterAsync(RegisterUserDto registerUserDto)
    {
        if (registerUserDto.Role == RoleNames.HotelAdmin && !registerUserDto.AssociatedHotelId.HasValue)
            return Result<RegisteredUserDto>.BadRequest(new Error(
                ErrorCodes.Validation, "HotelId is Required"));

        ApplicationUser user = new ApplicationUser
        {
            Email = registerUserDto.Email,
            FirstName = registerUserDto.FirstName,
            LastName = registerUserDto.LastName,
            UserName = registerUserDto.Email
        };

        bool role = await roleManager.RoleExistsAsync(registerUserDto.Role);

        if (!role)
        {
            return Result<RegisteredUserDto>
                .BadRequest(new Error(
                    ErrorCodes.BadRequest,
                    "Role Does Not Exist!"));
        }

        IdentityResult result = await userManager
            .CreateAsync(user, registerUserDto.Password);

        if (!result.Succeeded)
        {
            Error[] errors = result.Errors
                .Select(e => new Error(ErrorCodes.BadRequest, e.Description))
                .ToArray();

            return Result<RegisteredUserDto>.BadRequest(errors);
        }

        await userManager.AddToRoleAsync(user, registerUserDto.Role);

        if (registerUserDto.Role == "HotelAdmin")
        {
            HotelAdmin hotelAdmin = new HotelAdmin
            {
                HotelId = registerUserDto.AssociatedHotelId.GetValueOrDefault(),
                ApplicationUserId = user.Id
            };

            await context.AddAsync(hotelAdmin);
            await context.SaveChangesAsync();
        }

        RegisteredUserDto registeredUserDto = new RegisteredUserDto
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = registerUserDto.Role
        };

        return Result<RegisteredUserDto>.Success(registeredUserDto);
    }

    public async Task<Result<string>> LoginAsync(LoginUserDto loginUserDto)
    {
        ApplicationUser? user = await userManager.FindByEmailAsync(loginUserDto.Email);

        if (user == null)
        {
            return Result<string>.BadRequest(new Error(
                ErrorCodes.BadRequest, "Invalid Credentials"));
        }

        bool valid = await userManager.CheckPasswordAsync(user, loginUserDto.Password);

        if (!valid)
        {
            return Result<string>.BadRequest(new Error(
                ErrorCodes.BadRequest, "Invalid Credentials"));
        }

        string token = await GenerateToken(user);

        return Result<string>.Success(token);
    }

    public async Task<string> GenerateToken(ApplicationUser user)
    {
        //set basic user claims
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.Email, user.Email!),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Name,$"{user.FirstName} {user.LastName}")
        };

        //set user role claims
        IList<string> roles = await userManager.GetRolesAsync(user);
        List<Claim> roleClaims = roles.Select(x => new Claim(ClaimTypes.Role, x)).ToList();

        claims = claims.Union(roleClaims).ToList();


        //set jwt key Credentials
        SymmetricSecurityKey securityKey = new SymmetricSecurityKey
            (Encoding.UTF8.GetBytes(options.Value.Key));

        SigningCredentials credentials = new SigningCredentials(
            securityKey,
            SecurityAlgorithms.HmacSha256);

        //jwt creation

        JwtSecurityToken token = new JwtSecurityToken(
            issuer: options.Value.Issuer,
            audience: options.Value.Audience,
            claims: claims,
            expires: DateTime.UtcNow
            .AddMinutes(Convert.ToInt32(options.Value.DurationInMinutes)),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
