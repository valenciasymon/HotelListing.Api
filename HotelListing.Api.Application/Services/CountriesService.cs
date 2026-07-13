
using HotelListing.Api.Common.Constants;
using HotelListing.Api.Domain;
using HotelListing.Api.DTOs.CountryDto;
using HotelListing.Api.Common.Results;
using Microsoft.EntityFrameworkCore;
using HotelListing.Api.Common.Models.Paging;
using HotelListing.Api.Common.Models.Extensions;
using HotelListing.Api.DTOs.HotelDto;
using HotelListing.Api.Common.Models.Filtering;
using Microsoft.AspNetCore.JsonPatch;

namespace HotelListing.Api.Application.Services;

public class CountriesService(HotelListingDbContext context) : ICountriesService
{
    public async Task<Result<IEnumerable<GetCountriesDto>>> GetCountriesAsync(
        CountriesFilterParameter filters)
    {
        string searchTerm = filters.Search?.Trim().ToLowerInvariant() ?? string.Empty;

        IQueryable<Country> query = context.Countries;

        if (!string.IsNullOrWhiteSpace(filters.Search))
        {
            query = query.Where(c => EF.Functions.Like(c.Name, $"%{searchTerm}%")
            || EF.Functions.Like(c.ShortName, $"%{searchTerm}%"));
        }

        List<GetCountriesDto>countries = await query
            .AsNoTracking()
            .Select(c => new GetCountriesDto(
                c.Id,
                c.Name,
                c.ShortName))
            .ToListAsync();

        return Result<IEnumerable<GetCountriesDto>>.Success(countries);
    }

    
    public async Task<Result<GetCountryDto>> GetCountryAsync(int id)
    {
        GetCountryDto? countryDto = await context.Countries
            .AsNoTracking()
            .Where(c => c.Id == id)
            .Select(c => new GetCountryDto(
                c.Id,
                c.Name,
                c.ShortName,
                c.Hotels.Select(h => new CountryHotelsDto
            (h.Name, h.Address, h.Rating)).ToList()))
            .FirstOrDefaultAsync();

        return countryDto == null
            ? Result<GetCountryDto>.NotFound(new Error(
                ErrorCodes.NotFound,
                "Country Not Found"))
            : Result<GetCountryDto>.Success(countryDto);
    }

    public async Task<Result> UpdateCountryAsync(int id, UpdateCountryDto countryDto)
    {
        try
        {
            if (id != countryDto.Id)
            {
                return Result.BadRequest(new Error(
                    ErrorCodes.Validation,
                    "Id Route Does Not Match payload Id"));
            }

            Country? country = await context.Countries.FindAsync(id);

            if (country == null)
            {
                return Result.NotFound(new Error(
                    ErrorCodes.NotFound, 
                    "Country Not Found"));
            }

            bool duplicateName = await context.Countries.AnyAsync(
                c => c.Id != id && c.Name.ToLower().Trim() == 
                countryDto.Name.Trim().ToLower());

            if (duplicateName)
            {
                return Result.Conflict(new Error(
                    ErrorCodes.Conflict,
                    $"Country With The Name of {countryDto.Name} Already Exist! "));
            }

            country.Name = countryDto.Name;
            country.ShortName = countryDto.ShortName;

            await context.SaveChangesAsync();
            return Result.Success();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await CountryExists(id))
            {
                return Result.NotFound(new Error(
                    ErrorCodes.NotFound,
                    "Country Not Found"));
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

    public async Task<Result<GetCountryDto>> CreateCountryAsync(CreateCountryDto countryDto)
    {
        try
        {
            bool exists = await context.Countries
                .AsNoTracking()
                .AnyAsync(c => c.Name.ToLower().Trim() ==
                countryDto.Name.ToLower().Trim());

            if (exists)
            {
                return Result<GetCountryDto>.Conflict(new Error(
                    ErrorCodes.Conflict,
                    $"Country With The Name of {countryDto.Name} Already Exists!"));
            }

            Country country = new Country
            {
                Name = countryDto.Name,
                ShortName = countryDto.ShortName
            };

            context.Countries.Add(country);
            await context.SaveChangesAsync();

            GetCountryDto getCountryDto = new GetCountryDto(
                country.Id,
                country.Name,
                country.ShortName,
                []);

            return Result<GetCountryDto>.Success(getCountryDto);
        }
        catch (Exception)
        {
            return Result<GetCountryDto>.Failure(new Error(
                ErrorCodes.Failure,
                "Unexpected Server Error"));
        }
    }

    public async Task<Result> DeleteCountryAsync(int id)
    {
        try
        {
            int rows = await context.Countries
            .Where(c => c.Id == id)
            .ExecuteDeleteAsync();

            if (rows == 0)
            {
                return Result.NotFound(new Error(
                    ErrorCodes.NotFound,
                    "Country Not Found"));
            }

            return Result.Success();
        }
        catch (Exception)
        {
            return Result.Failure(new Error(
                ErrorCodes.Failure,
                "Unexpected Server Error"));
        }
    }

    public async Task<Result<PagedResult<GetHotelDto>>> GetCountriesHotelAsync(
        int countryId,
        PaginationParameters paginationParameters,
        HotelFilterParameters filters)
    {
        bool countryExist = await CountryExists(countryId);

        if (!countryExist) return Result<PagedResult<GetHotelDto>>.NotFound(new Error(
            ErrorCodes.NotFound, "Country Not Found"));

        IQueryable<Hotel> query = context.Hotels
            .Where(h => h.CountryId == countryId);

        if (filters.MinRating.HasValue)
            query = query.Where(h => h.Rating >= filters.MinRating);

        if (filters.MaxRating.HasValue)
            query = query.Where(h => h.Rating <= filters.MaxRating);

        if (filters.MinPrice.HasValue)
            query = query.Where(h => h.PerNightRate >= filters.MinPrice);

        if (filters.MaxPrice.HasValue)
            query = query.Where(h => h.PerNightRate <= filters.MaxPrice);

        if (!string.IsNullOrWhiteSpace(filters.Location))
            query = query.Where(h => h.Address.ToLower().Contains(filters.Location.ToLower()));

        if (!string.IsNullOrEmpty(filters.Search))
            query = query.Where(h => EF.Functions.Like(h.Name, $"%{filters.Search.Trim()}%")
            || EF.Functions.Like(h.Address,$"%{filters.Search.Trim()}%"));

        query = filters.SortBy?.Trim().ToLowerInvariant() switch
        {
            "name" => filters.SortDescending ?
                query.OrderByDescending(h => h.Name) : query.OrderBy(h => h.Name),
            "price" => filters.SortDescending ?
                query.OrderByDescending(h => h.PerNightRate) : query.OrderBy(h => h.PerNightRate),
            "address" => filters.SortDescending ?
                query.OrderByDescending(h => h.Address) : query.OrderBy(h => h.Address),
            _ => query.OrderBy(h => h.Name)
        };

        PagedResult<GetHotelDto> hotels = await query
            .AsNoTracking()
            .Select(h => new GetHotelDto(
                h.Id,
                h.Name,
                h.Address,
                h.Rating,
                h.PerNightRate,
                h.CountryId,
                h.Country != null ? h.Country.Name : null))
            .ToPagedResult(paginationParameters);

        return Result<PagedResult<GetHotelDto>>.Success(hotels);
    }

    public async Task<Result> PatchCountryAsync(int id, JsonPatchDocument<UpdateCountryDto> patchDoc)
    {
        Country? country = await context.Countries.FindAsync(id);

        if (country == null) return Result.NotFound(new Error(
            ErrorCodes.NotFound, "Country Not Found"));

        UpdateCountryDto updateCountryDto = new UpdateCountryDto
        {
            Id = country.Id,
            Name = country.Name,
            ShortName = country.ShortName
        };

        patchDoc.ApplyTo(updateCountryDto);

        if (updateCountryDto.Id != id) return Result.BadRequest(new Error(
            ErrorCodes.Validation, "Cannot Modify Id Field"));

        bool duplicateName = await context.Countries.AnyAsync(
            c => c.Id != id &&
            c.Name.Trim().ToLower() == updateCountryDto.Name.Trim().ToLower());

        if (duplicateName) return Result.Conflict(new Error(
            ErrorCodes.Conflict, "A Country with The Same Name Exist"));

        country.Name = updateCountryDto.Name;
        country.ShortName = updateCountryDto.ShortName;

        await context.SaveChangesAsync();
        return Result.Success();
    }

    private async Task<bool> CountryExists(int id)
    {
        return await context.Countries
            .AsNoTracking()
            .AnyAsync(c => c.Id == id);
    }

}
