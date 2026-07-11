

using HotelListing.Api.Common.Constants;
using HotelListing.Api.Domain;
using HotelListing.Api.DTOs.HotelDto;
using HotelListing.Api.Common.Results;
using Microsoft.EntityFrameworkCore;
using HotelListing.Api.Common.Models.Paging;
using HotelListing.Api.Common.Models.Extensions;
using HotelListing.Api.Common.Models.Filtering;

namespace HotelListing.Api.Application.Services;

public class HotelsService(HotelListingDbContext context) : IHotelsService
{
    public async Task<Result<PagedResult<GetHotelsDto>>> GetHotelsAsync(
        PaginationParameters paginationParameters, HotelFilterParameters filters)
    {
        IQueryable<Hotel> query = context.Hotels;

        if (filters.MinRating.HasValue)
            query = query.Where(q => q.Rating >= filters.MinRating);

        if (filters.MaxRating.HasValue)
            query = query.Where(q => q.Rating <= filters.MaxRating);

        if (filters.MinPrice.HasValue)
            query = query.Where(q => q.PerNightRate >= filters.MinPrice);

        if (filters.MaxPrice.HasValue)
            query = query.Where(q => q.PerNightRate <= filters.MaxPrice);

        if (!string.IsNullOrWhiteSpace(filters.Location))
            query = query.Where(h => EF.Functions.Like(h.Address,$"%{filters.Location.Trim()}%"));

        if (!string.IsNullOrEmpty(filters.Search))
            query = query.Where(h => EF.Functions.Like(h.Name, $"%{filters.Search.Trim()}%")||
            EF.Functions.Like(h.Address,$"%{filters.Search.Trim()}%"));

        query = filters.SortBy?.ToLower() switch
        {
            "name" => filters.SortDescending ?
                query.OrderByDescending(h => h.Name) : query.OrderBy(h => h.Name),
            "address" => filters.SortDescending ?
                query.OrderByDescending(h => h.Address) : query.OrderBy(h => h.Address),
            "price" => filters.SortDescending ? 
                query.OrderByDescending(h => h.PerNightRate) : query.OrderBy(h => h.PerNightRate),
            _ => query.OrderBy(h => h.Name)
        };

        PagedResult<GetHotelsDto> hotels = await query
            .AsNoTracking()
            .Select(h => new GetHotelsDto(
                h.Id,
                h.Name,
                h.Address,
                h.Rating,
                h.PerNightRate))
            .ToPagedResult(paginationParameters);

        return Result<PagedResult<GetHotelsDto>>.Success(hotels);
    }

    public async Task<Result<GetHotelDto>> GetHotelAsync(int id)
    {
        GetHotelDto? hotel = await context.Hotels
            .AsNoTracking()
            .Where(h => h.Id == id)
            .Select(h => new GetHotelDto(
                h.Id,
                h.Name,
                h.Address,
                h.Rating,
                h.PerNightRate,
                h.CountryId,
                h.Country != null ? h.Country.Name : null))
            .FirstOrDefaultAsync();

        return hotel == null
            ? Result<GetHotelDto>.NotFound(new Error(ErrorCodes.NotFound, "Hotel Not Found"))
            : Result<GetHotelDto>.Success(hotel);
    }

    public async Task<Result> UpdateHotelAsync(int id, UpdateHotelDto hotelDto)
    {
        try
        {
            if (id != hotelDto.Id)
            {
                return Result.BadRequest(new Error(
                    ErrorCodes.Validation, "Id Route Does Not Match payload Id"));
            }

            Hotel? hotel = await context.Hotels.FindAsync(id);

            if (hotel == null)
            {
                return Result.NotFound(new Error(ErrorCodes.NotFound, "Hotel Not Found"));
            }

            bool isDuplicate = await context.Hotels
                .AnyAsync(
                h => h.Id != id && h.Name.Trim().ToLower() ==
                hotelDto.Name.ToLower().Trim());

            if (isDuplicate)
            {
                return Result.Conflict(new Error(
                    ErrorCodes.Conflict, 
                    $"Hotel With The Name of {hotelDto.Name} Already Exist! "));
            }

            bool countryExists = await context.Countries
                .AnyAsync(c => c.Id == hotelDto.CountryId);

            if (!countryExists)
            {
                return Result.NotFound(new Error(
                    ErrorCodes.NotFound, $"Country '{hotelDto.CountryId}' Was Not Found"));
            }

            hotel.Name = hotelDto.Name;
            hotel.Address = hotelDto.Address;
            hotel.Rating = hotelDto.Rating;
            hotel.PerNightRate = hotelDto.PerNightRate;
            hotel.CountryId = hotelDto.CountryId;

            await context.SaveChangesAsync();
            return Result.Success();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await HotelExists(id))
            {
                return Result.NotFound(new Error(ErrorCodes.NotFound, "Hotel Not Found"));
            }
            else
            {
                throw;
            }
        }
        catch (Exception)
        {
            return Result.Failure();
        }

    }

    public async Task<Result<GetHotelDto>> CreateHotelAsync(CreateHotelDto hotelDto)
    {
        try
        {
            bool exist = await context.Hotels
                .AnyAsync(h => h.Name.ToLower().Trim() == hotelDto.Name.ToLower().Trim());

            if (exist)
            {
                return Result<GetHotelDto>.Conflict(new Error(
                    ErrorCodes.Conflict,
                    $"Hotel With The Name of {hotelDto.Name} Already Exists!"));
            }

            bool countryExists = await context.Countries
                .AnyAsync(c => c.Id == hotelDto.CountryId);

            if (!countryExists)
            {
                return Result<GetHotelDto>.NotFound(new Error(
                    ErrorCodes.NotFound, $"Country '{hotelDto.CountryId}' Was Not Found"));
            }

            Hotel hotel = new Hotel
            {
                Name = hotelDto.Name,
                Address = hotelDto.Address,
                Rating = hotelDto.Rating,
                PerNightRate = hotelDto.PerNightRate,
                CountryId = hotelDto.CountryId
            };

            context.Hotels.Add(hotel);
            await context.SaveChangesAsync();

            await context.Entry(hotel)
                .Reference(h => h.Country)
                .LoadAsync();

            GetHotelDto getHotelDto = new GetHotelDto(
                hotel.Id,
                hotel.Name,
                hotel.Address,
                hotel.Rating,
                hotel.PerNightRate,
                hotel.CountryId,
                hotel.Country != null ? hotel.Country.Name : null);

            return Result<GetHotelDto>.Success(getHotelDto);
        }
        catch (Exception)
        {
            return Result<GetHotelDto>.Failure(new Error(
                ErrorCodes.Failure, "Unexpected Server Error"));
        }


    }

    public async Task<Result> DeleteHotelAsync(int id)
    {
        try
        {
            int rows = await context.Hotels
            .Where(h => h.Id == id)
            .ExecuteDeleteAsync();

            if (rows == 0)
            {
                return Result.NotFound(new Error(ErrorCodes.NotFound, "Hotel Not Found"));
            }

            return Result.Success();
        }
        catch (Exception)
        {
            return Result.Failure(new Error(ErrorCodes.Failure,"Unexpected Server Error"));
        }
    }


    private async Task<bool> HotelExists(int id)
    {
        return await context.Hotels.AnyAsync(h => h.Id == id);
    }
}
