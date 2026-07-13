
using HotelListing.Api.Common.Constants;
using HotelListing.Api.Domain;
using HotelListing.Api.Application.Contracts;
using HotelListing.Api.Handlers;
using HotelListing.Api.Application.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using HotelListing.Api.Common.Models.Config;

namespace HotelListing.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {

            var builder = WebApplication.CreateBuilder(args);

            //adding services to ioc container
            var strconn = builder.Configuration.GetConnectionString("HotelListingDbConnectionString");
            builder.Services.AddDbContextPool<HotelListingDbContext>(options =>{
                options.UseSqlServer(strconn, sqlOptions =>
                {
                    sqlOptions.CommandTimeout(30);
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount:3,
                        maxRetryDelay: TimeSpan.FromSeconds(5),
                        errorNumbersToAdd: null);
                });

                if (builder.Environment.IsDevelopment())
                { 
                    options.EnableDetailedErrors();
                    options.EnableSensitiveDataLogging();
                }

            },poolSize: 128);
            // Add services to the container.
            builder.Services.AddIdentityApiEndpoints<ApplicationUser>()
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<HotelListingDbContext>();

            builder.Services.AddHttpContextAccessor();

            builder.Services.Configure<JwtSettings>(
                builder.Configuration.GetSection("JwtSettings"));

            var jwtSettings = builder.Configuration
                .GetSection("JwtSettings")
                .Get<JwtSettings>() ?? new JwtSettings();

            if(string.IsNullOrEmpty(jwtSettings.Key))
            {
                throw new InvalidOperationException("JwtSetting:Key is Not Configured");
            }

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
                        ValidIssuer = jwtSettings.Issuer,
                        ValidAudience = jwtSettings.Audience,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                            jwtSettings.Key)),
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
                .AddNewtonsoftJson()
                .AddJsonOptions(options => 
                { 
                    options.JsonSerializerOptions.ReferenceHandler = 
                    System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles; 
                }); 
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            //builder.Services.AddMemoryCache();
            builder.Services.AddOutputCache(options =>
            {
                options.AddPolicy(CacheConstants.AuthenticatedUserCachingPolicy, builder =>
                {
                    builder.AddPolicy<AuthenticatedUserCachingPolicy>()
                    .SetCacheKeyPrefix(CacheConstants.AuthenticatedUserCachingPolicyTag);
                }, true);
            });

            var app = builder.Build();

            app.MapGroup("api/defaultauth").MapIdentityApi<ApplicationUser>();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.UseOutputCache();

            app.MapControllers();

            app.Run();
        }
    }
}
