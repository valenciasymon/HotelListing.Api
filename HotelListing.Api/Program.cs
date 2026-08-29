
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
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using Serilog;
using Serilog.Events;
using Serilog.Extensions.Hosting;
using System.IdentityModel.Tokens.Jwt;
using HotelListing.Api.Middleware;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using System.Text.Json;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using HealthChecks.UI.Client;
using Asp.Versioning;
using Microsoft.OpenApi;
using System.Reflection;
using Swashbuckle.AspNetCore.Filters;
using HotelListing.Api.Filters;

namespace HotelListing.Api
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
                .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
                .CreateBootstrapLogger();
            try
            {
                Log.Information("Starting HotelListing.API");
                var builder = WebApplication.CreateBuilder(args);

                builder.Host.UseSerilog((context, services, configuration) => configuration
                    .ReadFrom.Configuration(context.Configuration)
                    .ReadFrom.Services(services)
                );

                //adding services to ioc container
                var strconn = builder.Configuration.GetConnectionString("HotelListingDbConnectionString");
                builder.Services.AddDbContextPool<HotelListingDbContext>(options => {
                    options.UseSqlServer(strconn, sqlOptions =>
                    {
                        sqlOptions.CommandTimeout(30);
                        sqlOptions.EnableRetryOnFailure(
                            maxRetryCount: 3,
                            maxRetryDelay: TimeSpan.FromSeconds(5),
                            errorNumbersToAdd: null);
                    });

                    if (builder.Environment.IsDevelopment())
                    {
                        options.EnableDetailedErrors();
                        options.EnableSensitiveDataLogging();
                    }

                }, poolSize: 128);

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

                if (string.IsNullOrEmpty(jwtSettings.Key))
                {
                    Log.Fatal("JwtSettings:Key is Not Configured");

                    throw new InvalidOperationException("JwtSettings:Key is Not Configured");
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

                builder.Services.AddScoped<ICountriesService, CountriesService>();
                builder.Services.AddScoped<IHotelsService, HotelsService>();
                builder.Services.AddScoped<IUserService, UserService>();
                builder.Services.AddScoped<IApiKeyValidatorService, ApiKeyValidatorService>();
                builder.Services.AddScoped<IBookingService, BookingService>();

                builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
                builder.Services.AddProblemDetails();

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

                builder.Services.AddRateLimiter(options =>
                {
                    //fixed
                    options.AddFixedWindowLimiter(RateLimitingConstants.FixedPolicy, opt =>
                    {
                        opt.Window = TimeSpan.FromMinutes(1);
                        opt.PermitLimit = 50;
                        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                        opt.QueueLimit = 5;
                    });

                    //sliding
                    options.AddPolicy(RateLimitingConstants.PerUserPolicy, context =>
                    {
                        string username = context.User?.Identity?.Name
                            ?? RateLimitingConstants.Anon;

                        return RateLimitPartition.GetSlidingWindowLimiter(username,
                            _ => new SlidingWindowRateLimiterOptions
                            {
                                Window = TimeSpan.FromMinutes(1),
                                PermitLimit = 50,
                                SegmentsPerWindow = 6,
                                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                                QueueLimit = 3
                            });
                    });

                    //global limiter
                    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                    {
                        string ipAddress = context.Connection.RemoteIpAddress?.ToString()
                            ?? RateLimitingConstants.Unknown;

                        return RateLimitPartition.GetFixedWindowLimiter(ipAddress,
                            _ => new FixedWindowRateLimiterOptions
                            {
                                Window = TimeSpan.FromMinutes(1),
                                PermitLimit = 200,
                                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                                QueueLimit = 10
                            });
                    });

                    options.OnRejected = async (context, cancellationToken) =>
                    {
                        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                        {
                            context.HttpContext.Response.Headers.RetryAfter
                             = retryAfter.TotalSeconds.ToString();
                        }

                        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                        context.HttpContext.Response.ContentType = "application/json";

                        await context.HttpContext.Response.WriteAsJsonAsync(new
                        {
                            error = "Too Many Requests",
                            message = "Rate Limit exceeded. Please try again later.",
                            retryAfter = retryAfter.TotalSeconds
                        }, cancellationToken: cancellationToken);
                    };
                });

                builder.Services.AddHealthChecks()
                    .AddCheck("self", () => HealthCheckResult.Healthy("Application is Running"),
                        tags:["api"])
                    .AddDbContextCheck<HotelListingDbContext>(
                        name: "Database",
                        failureStatus: HealthStatus.Unhealthy,
                        tags: ["db","sql"]);

                builder.Services.AddHealthChecksUI(setup =>
                {
                    setup.SetEvaluationTimeInSeconds(30);
                    setup.MaximumHistoryEntriesPerEndpoint(50);
                    setup.AddHealthCheckEndpoint("HotelListing API", "/healthz");

                }).AddInMemoryStorage();

                builder.Services.AddApiVersioning(options =>
                {
                    options.AssumeDefaultVersionWhenUnspecified = true;
                    options.DefaultApiVersion = new ApiVersion(1, 0);
                    options.ReportApiVersions = true;
                    options.ApiVersionReader = new UrlSegmentApiVersionReader();
                }).AddApiExplorer(options =>
                {
                    options.GroupNameFormat = "'v'VVV";
                    options.SubstituteApiVersionInUrl = true;
                });

                //SWAGGER
                builder.Services.AddEndpointsApiExplorer();
                builder.Services.AddSwaggerGen(options =>
                {
                    //API Information
                    options.SwaggerDoc("v1", new OpenApiInfo
                    {
                        Version = "v1",
                        Title = "HotelListing API",
                        Description = "API for managing Hotels, Countries , And Bookings",
                        Contact = new OpenApiContact
                        {
                            Name = "Support Team",
                            Email = "support@hotellisting.com"
                        },
                        License = new OpenApiLicense
                        {
                            Name = "MIT License",
                            Url = new Uri("https://opensource.org/licenses/MIT")
                        }
                    });

                    options.SwaggerDoc("v2", new OpenApiInfo
                    {
                        Version = "v2",
                        Title = "Hotel Listing Api V2",
                        Description = "Version 2 of the Hotel Listing API with enchanced Features"
                    });

                    options.DocInclusionPredicate((documentName, apiDescription) =>
                    {
                        return apiDescription.GroupName == null ||
                            string.Equals(
                                apiDescription.GroupName,
                                documentName,
                                StringComparison.OrdinalIgnoreCase);
                    });

                    //include xml comments
                    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                    if (File.Exists(xmlPath))
                    {
                        options.IncludeXmlComments(xmlPath);
                    }


                    //enable annotations
                    options.EnableAnnotations();

                    //security Definitions
                    // Jwt Bearer
                    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                    {
                        Description = "Jwt Authorization Header using the Bearer Scheme. Enter 'Bearer'",
                        Name = "Authorization",
                        In = ParameterLocation.Header,
                        Type = SecuritySchemeType.Http,
                        Scheme = JwtBearerDefaults.AuthenticationScheme,
                        BearerFormat = "JWT"
                    });

                    //API Key Authentication
                    options.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
                    {
                        Description = "Api Key needed to access the Endpoints. X-Api-Key: {API Key}",
                        In = ParameterLocation.Header,
                        Name = "X-API-Key",
                        Type = SecuritySchemeType.ApiKey
                    });

                    //Basic Auth
                    options.AddSecurityDefinition("Basic", new OpenApiSecurityScheme
                    {
                        Description = "Basic Authentication Header",
                        Name = "Authorization",
                        In = ParameterLocation.Header,
                        Type = SecuritySchemeType.Http,
                        Scheme = "Basic"
                    });

                    //add security Requirements
                    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
                    {
                        [new OpenApiSecuritySchemeReference("Bearer",document)] = [] // dictionary
                    });

                    //add operation filters for examples
                    options.ExampleFilters();

                    //custom operation filters  for handling multiple auth schemes
                    options.OperationFilter<Filters.SecurityRequirementsOperationFilter>();

                    //order actions by method
                    options.OrderActionsBy(apiDesc => $"{apiDesc.RelativePath}_{apiDesc.HttpMethod}");
                });

                builder.Services.AddSwaggerExamplesFromAssemblyOf<Program>();

                var app = builder.Build();

                app.UseExceptionHandler();

                //logging request w/serilog
                app.UseSerilogRequestLogging(options =>
                {
                    options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded " +
                    "{StatusCode} in {Elapsed:0.0000}ms";

                    options.GetLevel = (httpContext, elapsed, ex) => ex != null
                        ? LogEventLevel.Error
                        : httpContext.Response.StatusCode >= 500
                            ? LogEventLevel.Error
                            : httpContext.Response.StatusCode >= 400
                                ? LogEventLevel.Warning
                                : LogEventLevel.Information;

                    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
                    {
                        diagnosticContext.Set("UserName",
                            httpContext.User?.Identity?.Name ?? "Anonymous");

                        diagnosticContext.Set("RemoteIP",
                            httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown");

                        if (httpContext.User?.Identity?.IsAuthenticated == true)
                        {
                            diagnosticContext.Set("UserId",
                                httpContext.User?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                                ?? "unknown");
                        }
                    };
                });

                app.MapGroup("api/defaultauth").MapIdentityApi<ApplicationUser>();

                // Configure the HTTP request pipeline.
                if (app.Environment.IsDevelopment())
                {
                    app.MapOpenApi();
                    app.UseSwagger();
                    app.UseSwaggerUI(options =>
                    {
                        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Hotel Listing API V1");
                        options.SwaggerEndpoint("/swagger/v2/swagger.json", "Hotel Listing API V2");
                        options.RoutePrefix = "swagger";
                        options.DocumentTitle = "Hotel Listing API Documentation";
                        options.DisplayRequestDuration();
                        options.EnableDeepLinking();
                        options.EnableFilter();
                        options.ShowExtensions();
                        options.EnableValidator();
                    });
                }

                app.UseHttpsRedirection();

                //app.MapHealthChecks("/healthz", new HealthCheckOptions
                //{
                //    ResponseWriter = async (context, report) =>
                //    {
                //        context.Response.ContentType = "application/json";

                //        var response = new
                //        {
                //            status = report.Status.ToString(),
                //            checks = report.Entries.Select(entry => new
                //            {
                //                name = entry.Key,
                //                status = entry.Value.Status.ToString(),
                //                description = entry.Value.Description,
                //                duration = entry.Value.Duration.TotalMicroseconds,
                //                exception = entry.Value.Exception?.Message,
                //                data = entry.Value.Data
                //            }),
                //            totalDuration = report.TotalDuration.TotalMicroseconds

                //        };

                //        await context.Response.WriteAsync(JsonSerializer.Serialize(
                //            response, new JsonSerializerOptions
                //            {
                //                WriteIndented = true
                //            }));
                //    }
                //});

                //healthcheck UI
                app.MapHealthChecks("healthz", new HealthCheckOptions
                {
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                });

                app.MapHealthChecks("/healthz/live", new HealthCheckOptions
                {
                    Predicate = check => false
                });

                app.MapHealthChecks("/healthz/ready", new HealthCheckOptions
                {
                    Predicate = check => check.Tags.Contains("db")
                });

                app.MapHealthChecksUI(options =>
                {
                    options.UIPath = "/healthchecks-ui";
                    options.ApiPath = "/healthchecks-api";
                });

                app.UseRateLimiter();

                app.UseAuthorization();

                app.UseOutputCache();

                app.MapControllers();

                Log.Information("HotelListing API Started Successfully");

                app.Run();
            }
            catch (Exception e)
            {
                Log.Fatal(e, "Application Terminated Unexpectedly");
            }
            finally
            {
                Log.CloseAndFlush();
            }

        }
    }
}
