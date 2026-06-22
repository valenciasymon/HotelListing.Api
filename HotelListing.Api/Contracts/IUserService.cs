using HotelListing.Api.DTOs.AuthDto;
using HotelListing.Api.Results;

namespace HotelListing.Api.Contracts
{
    public interface IUserService
    {
        Task<Result<string>> LoginAsync(LoginUserDto loginUserDto);
        Task<Result<RegisteredUserDto>> RegisterAsync(RegisterUserDto registerUserDto);
    }
}