using HotelListing.Api.Common.Constants;
using HotelListing.Api.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace HotelListing.Api.AuthorizationFilters;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public sealed class HotelSystemAdminAttribute : TypeFilterAttribute
{
    public HotelSystemAdminAttribute() : base(typeof(HotelOrSytemAdminFilter))
    {
        
    }
}

public class HotelOrSytemAdminFilter(HotelListingDbContext dbContext) :
    IAsyncAuthorizationFilter
{
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var httpUser = context.HttpContext.User;

        if(httpUser?.Identity?.IsAuthenticated == false)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        if (httpUser!.IsInRole(RoleNames.Admin))
        {
            return;
        }

        string? userId = httpUser.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ??
            httpUser.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;

        if (string.IsNullOrEmpty(userId))
        {
            context.Result = new ForbidResult();
            return;
        }

        context.RouteData.Values.TryGetValue("hotelId",out object? hotelIdObj);
        int.TryParse(hotelIdObj?.ToString(), out int parsedHotelId);

        bool hotelExists = await dbContext.Hotels.AnyAsync(h => h.Id == parsedHotelId);

        if(!hotelExists)
        {
            context.Result = new ForbidResult();
            return;
        }

        bool isHotelAdmin = await dbContext.HotelAdmins.AnyAsync(
            a => a.ApplicationUserId == userId
            && a.HotelId == parsedHotelId);

        if (!isHotelAdmin)
        {
            context.Result = new ForbidResult();
            return;
        }
    }
}