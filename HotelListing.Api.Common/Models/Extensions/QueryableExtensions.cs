
using HotelListing.Api.Common.Models.Paging;
using Microsoft.EntityFrameworkCore;

namespace HotelListing.Api.Common.Models.Extensions;

public static class QueryableExtensions
{
    public static async Task<PagedResult<T>> ToPagedResult<T>(
        this IQueryable<T> source,
        PaginationParameters paginationParameters)
    {
        int totalCount = await source.CountAsync();

        List<T> items = await source
            .Skip((paginationParameters.PageNumber - 1) * paginationParameters.PageSize)
            .Take(paginationParameters.PageSize)
            .ToListAsync();

        int totalPages = (int)Math.Ceiling(totalCount / (double)paginationParameters.PageSize);

        PaginationMetaData metadata = new PaginationMetaData
        {
            CurrentPage = paginationParameters.PageNumber,
            PageSize = paginationParameters.PageSize,
            TotalCount = totalCount,
            TotalPages = totalPages,
            HasNext = paginationParameters.PageNumber < totalPages,
            HasPrevious = paginationParameters.PageNumber > 1
        };

        return new PagedResult<T>
        {
            Data = items,
            MetaData = metadata
        };
    }
}
