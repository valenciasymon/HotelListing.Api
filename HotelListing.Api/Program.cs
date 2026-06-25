

using HotelListing.Api.Constants;
using HotelListing.Api.Contracts;
using HotelListing.Api.Data;
using HotelListing.Api.Handlers;
using HotelListing.Api.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace HotelListing.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {

            var builder = WebApplication.CreateBuilder(args);

            //adding services to ioc container
            var strconn = builder.Configuration.GetConnectionString("HotelListingDbConnectionString");
            builder.Services.AddDbContext<HotelListingDbContext>
                (options => options.UseSqlServer(strconn));
            // Add services to the container.
            builder.Services.AddIdentityApiEndpoints<ApplicationUser>()
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<HotelListingDbContext>();

            builder.Services.AddHttpContextAccessor();

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
                .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>
                    (AuthenticationDefaults.BasicScheme, _ => { })
                .AddScheme<AuthenticationSchemeOptions, ApiKeyAuthenticationHandler>
                    (AuthenticationDefaults.ApiKeyScheme, _ => { })
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
                        ValidAudience = builder.Configuration["JwtSettings:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                            builder.Configuration["JwtSettings:Key"])),
                        ClockSkew = TimeSpan.Zero

                    };
                });

            builder.Services.AddAuthorization();

            builder.Services.AddScoped<ICountriesService,CountriesService>();
            builder.Services.AddScoped<IHotelsService, HotelsService>();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IApiKeyValidatorService, ApiKeyValidatorService>();
            builder.Services.AddScoped<IBookingService, BookingService>();

            builder.Services.AddControllers()
                .AddJsonOptions(options => 
                { options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles; }); 
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            var app = builder.Build();

            app.MapGroup("api/defaultauth").MapIdentityApi<ApplicationUser>();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
