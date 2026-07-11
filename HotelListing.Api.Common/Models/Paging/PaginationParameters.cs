
using System.ComponentModel.DataAnnotations;

namespace HotelListing.Api.Common.Models.Paging;

public class PaginationParameters
{
    private const int MaxPageSize = 50;
    private int _pagesize = 10;

    [Range(1, int.MaxValue, ErrorMessage = "Page Number must be greater than 0")]
    public int PageNumber { get; set; } = 1;

    [Range(1,MaxPageSize,ErrorMessage = "Page size must be between 1 and 50")]
    public int PageSize
    {
        get => _pagesize;
        set => _pagesize = value > MaxPageSize ? MaxPageSize : value;
    }

}
