using HotelListing.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace HotelListing.Api.AuthenticationFilters;

public class HotelSystemAdminAttribute(Type type) : TypeFilterAttribute(type)
{

}

public class HotelOrSytemAdminFilter(HotelListingDbContext context) : IAsyncAuthorizationFilter
{
    public Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var httpUser = context.HttpContext.User;

        if(httpUser?.Identity?.IsAuthenticated == false)
        {
            context.Result = new UnauthorizedResult();
            return;
        }
    }
}