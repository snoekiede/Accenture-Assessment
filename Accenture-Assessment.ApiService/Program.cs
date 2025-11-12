using System.Threading.RateLimiting;
using Accenture_Assessment.Contracts.Dtos;
using Accenture_Assessment.Data.Contexts;
using Accenture_Assessment.Data.Interfaces.Repositories;
using Accenture_Assessment.Data.Interfaces.Services;
using Accenture_Assessment.Data.Repositories;
using Accenture_Assessment.Data.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Polly;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Configure SQL Server with connection resiliency
builder.AddSqlServerDbContext<HolidayDbContext>("holidaysdb");

builder.Services.AddScoped<ICountryRepository, CountryRepository>();
builder.Services.AddScoped<IHolidayRepository, HolidayRepository>();

// Configure HttpClient with settings from configuration
var nagerApiConfig = builder.Configuration.GetSection("ExternalApis:NagerDate");
builder.Services.AddHttpClient<IHolidayApiClient, HolidayApiClient>(client =>
{
    client.BaseAddress = new Uri(nagerApiConfig["BaseUrl"] ?? "https://date.nager.at/api/v3/");
    client.DefaultRequestHeaders.Add("User-Agent", "Accenture-Assessment-Exercise");
    client.Timeout = TimeSpan.FromSeconds(30);
})
.AddTransientHttpErrorPolicy(policy =>
    policy.WaitAndRetryAsync(
        retryCount: nagerApiConfig.GetValue("MaxRetries", 3),
     sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(
            Math.Pow(nagerApiConfig.GetValue("RetryDelaySeconds", 2), retryAttempt))));

builder.Services.AddScoped<IHolidayDataService, HolidayDataService>();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        var allowedOrigins = builder.Configuration["AllowedOrigins"]?.Split(',')
                 ?? ["https://localhost:7148"];

        policy.WithOrigins(allowedOrigins)
     .AllowAnyMethod()
   .AllowAnyHeader()
          .WithExposedHeaders("X-Pagination");
    });
});

// Add Rate Limiting
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(
partitionKey: context.User.Identity?.Name ?? context.Request.Headers.Host.ToString(),
     factory: _ => new FixedWindowRateLimiterOptions
     {
         PermitLimit = builder.Configuration.GetValue("RateLimiting:PermitLimit", 100),
         Window = builder.Configuration.GetValue("RateLimiting:Window", TimeSpan.FromMinutes(1)),
         QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
         QueueLimit = 0
     }));

    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

// Add Health Checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<HolidayDbContext>("database")
    .AddUrlGroup(
    new Uri(nagerApiConfig["BaseUrl"] + "AvailableCountries"),
        name: "nager-api",
        timeout: TimeSpan.FromSeconds(5));

// Add Output Caching
builder.Services.AddOutputCache(options =>
{
    options.AddBasePolicy(b => b.Expire(TimeSpan.FromMinutes(10)));
});

builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();

var app = builder.Build();

// Apply migrations in development
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<HolidayDbContext>();

    // Use migrations instead of EnsureCreated
    try
    {
        await dbContext.Database.MigrateAsync();
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating the database.");
    }
}

// Configure middleware pipeline
app.UseExceptionHandler();
app.UseCors();
app.UseRateLimiter();
app.UseOutputCache();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Validation helper methods
static bool ValidateYear(int year, IConfiguration config)
{
    var minYear = config.GetValue("Validation:MinYear", 1900);
    var maxYear = config.GetValue("Validation:MaxYear", 2100);
    return year >= minYear && year <= maxYear;
}

static bool ValidateCountryCode(string code)
{
    return !string.IsNullOrWhiteSpace(code) &&
           code.Length == 2 &&
      code.All(char.IsUpper);
}

// API Endpoints with validation and caching
app.MapGet("/api/countries", async (IHolidayDataService service, bool forceSync = false) =>
{
    var countries = (await service.GetCountriesAsync(forceSync)).Select(x => new CountryDto
    {
        CountryCode = x.Code,
        Name = x.Name
    });
    return Results.Ok(countries);
})
.WithName("GetCountries")
.WithOpenApi()
.WithDescription("Get all countries. Set forceSync=true to refresh from external API.")
.CacheOutput(policy => policy.Expire(TimeSpan.FromHours(24)).Tag("countries"));

app.MapGet("/api/holidays/last-celebrated/{countryCode}", async (
    string countryCode,
    IHolidayDataService service) =>
{
    if (!ValidateCountryCode(countryCode))
        return Results.BadRequest("Invalid country code format. Must be 2 uppercase letters.");

    var holidays = await service.GetLastCelebratedHolidaysAsync(countryCode);
    var result = holidays.Select(h => new HolidayResultDto
    {
        Date = h.Date,
        Name = h.Name,
        LocalName = h.LocalName
    });
    return Results.Ok(result);
})
.WithName("GetLastCelebratedHolidays")
.WithOpenApi()
.WithDescription("Get the last three holidays celebrated in the country")
.CacheOutput(policy => policy.Expire(TimeSpan.FromHours(6)).SetVaryByRouteValue("countryCode"));

app.MapGet("/api/holidays/public-count/{year}", async (
    int year,
    IHolidayDataService service,
    IConfiguration config,
    [FromQuery] string[] countryCodes) =>
{
    if (!ValidateYear(year, config))
        return Results.BadRequest($"Year must be between {config.GetValue("Validation:MinYear", 1900)} and {config.GetValue("Validation:MaxYear", 2100)}.");

    if (countryCodes.Length == 0)
        return Results.BadRequest("At least one country code must be provided.");

    if (!countryCodes.All(ValidateCountryCode))
        return Results.BadRequest("Invalid country code format. All codes must be 2 uppercase letters.");

    var result = await service.GetPublicHolidaysCountByCountryAsync(year, countryCodes.ToList());

    var response = result.Select(kvp => new PublicHolidayCountDto
    {
        CountryCode = kvp.Key,
        PublicHolidaysCount = kvp.Value
    });

    return Results.Ok(response);
})
.WithName("GetPublicHolidaysCount")
.WithOpenApi()
.WithDescription("Get count of public holidays not falling on weekends for specified countries and year. Results sorted by count in descending order.")
.CacheOutput(policy => policy.Expire(TimeSpan.FromDays(1)).SetVaryByRouteValue("year").SetVaryByQuery("countryCodes"));

app.MapGet("/api/holidays/shared/{year}", async (
    int year,
  string countryCode1,
    string countryCode2,
    IHolidayDataService service,
    IConfiguration config) =>
{
    if (!ValidateYear(year, config))
        return Results.BadRequest($"Year must be between {config.GetValue("Validation:MinYear", 1900)} and {config.GetValue("Validation:MaxYear", 2100)}.");

    if (!ValidateCountryCode(countryCode1) || !ValidateCountryCode(countryCode2))
        return Results.BadRequest("Invalid country code format. Must be 2 uppercase letters.");

    if (countryCode1.Equals(countryCode2, StringComparison.OrdinalIgnoreCase))
        return Results.BadRequest("Country codes must be different.");

    var sharedHolidays = await service.GetSharedHolidayDatesAsync(year, countryCode1, countryCode2);

    return Results.Ok(sharedHolidays);
})
.WithName("GetSharedHolidays")
.WithOpenApi()
.WithDescription("Get deduplicated list of dates celebrated in both countries with local names for each country. Results sorted by date.")
.CacheOutput(policy => policy.Expire(TimeSpan.FromDays(1))
    .SetVaryByRouteValue("year")
    .SetVaryByQuery("countryCode1", "countryCode2"));


app.MapDefaultEndpoints();

app.Run();