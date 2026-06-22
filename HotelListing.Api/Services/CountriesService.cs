
using HotelListing.Api.Constants;
using HotelListing.Api.Data;
using HotelListing.Api.DTOs.CountryDto;
using HotelListing.Api.Results;
using Microsoft.EntityFrameworkCore;

namespace HotelListing.Api.Services;

public class CountriesService(HotelListingDbContext context) : ICountriesService
{
    public async Task<Result<IEnumerable<GetCountriesDto>>> GetCountriesAsync()
    {
        List<GetCountriesDto> countries = await context.Countries
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

            context.Entry(country).State = EntityState.Modified;

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


    private async Task<bool> CountryExists(int id)
    {
        return await context.Countries.AnyAsync(c => c.Id == id);
    }
}
