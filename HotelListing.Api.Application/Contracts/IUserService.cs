using HotelListing.Api.DTOs.AuthDto;
using HotelListing.Api.Common.Results;

namespace HotelListing.Api.Application.Contracts
{
    public interface IUserService
    {
        Task<Result<string>> LoginAsync(LoginUserDto loginUserDto);
        Task<Result<RegisteredUserDto>> RegisterAsync(RegisterUserDto registerUserDto);
    }
}