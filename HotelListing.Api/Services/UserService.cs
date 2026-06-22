using HotelListing.Api.Constants;
using HotelListing.Api.Contracts;
using HotelListing.Api.Data;
using HotelListing.Api.DTOs.AuthDto;
using HotelListing.Api.Results;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Text;

namespace HotelListing.Api.Services;

public class UserService(
    UserManager<ApplicationUser> userManager,
    IConfiguration configuration,
    RoleManager<IdentityRole> roleManager) : IUserService
{
    public async Task<Result<RegisteredUserDto>> RegisterAsync(RegisterUserDto registerUserDto)
    {
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
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Name,$"{user.FirstName} {user.LastName}")

        };

        //set user role claims
        IList<string> roles = await userManager.GetRolesAsync(user);
        List<Claim> roleClaims = roles.Select(x => new Claim(ClaimTypes.Role, x)).ToList();

        claims = claims.Union(roleClaims).ToList();


        //set jwt key Credentials
        SymmetricSecurityKey securityKey = new SymmetricSecurityKey
            (Encoding.UTF8.GetBytes(configuration["JwtSettings:Key"]));

        SigningCredentials credentials = new SigningCredentials(
            securityKey,
            SecurityAlgorithms.HmacSha256);

        //jwt creation

        JwtSecurityToken token = new JwtSecurityToken(
            issuer: configuration["JwtSettings:Issuer"],
            audience: configuration["JwtSettings:Audience"],
            claims: claims,
            expires: DateTime.UtcNow
            .AddMinutes(Convert.ToInt32(configuration["JwtSettings:DurationInMinutes"])),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
