using HotelListing.Api.Common.Results;
using HotelListing.Api.Application.Contracts;
using HotelListing.Api.DTOs.AuthDto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelListing.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class AuthController(IUserService userService) : BaseApiController
{
    [HttpPost("register")]
    public async Task<ActionResult<RegisteredUserDto>> Register(RegisterUserDto registerUserDto)
    {
        Result<RegisteredUserDto> user = await userService.RegisterAsync(registerUserDto);

        return ToActionResult(user);
    }

    [HttpPost("login")]
    public async Task<ActionResult<string>> Login(LoginUserDto loginUserDto)
    {
        Result<string> user = await userService.LoginAsync(loginUserDto);

        return ToActionResult(user);
    }
}
