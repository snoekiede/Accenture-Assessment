using Accenture_Assessment.Contracts.Dtos;
using Accenture_Assessment.Data.Contexts;
using Accenture_Assessment.Data.Interfaces.Repositories;
using Accenture_Assessment.Data.Interfaces.Services;
using Accenture_Assessment.Data.Repositories;
using Accenture_Assessment.Data.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

builder.AddSqlServerDbContext<HolidayDbContext>("holidaysdb");
builder.Services.AddScoped<ICountryRepository, CountryRepository>();
builder.Services.AddScoped<IHolidayRepository, HolidayRepository>();


builder.Services.AddHttpClient<IHolidayApiClient, HolidayApiClient>(client =>
{
    client.BaseAddress = new Uri("https://date.nager.at/api/v3/");
    client.DefaultRequestHeaders.Add("User-Agent", "Accenture-Assessment-Excercise");
});
// Add services to the container.
builder.Services.AddScoped<IHolidayDataService, HolidayDataService>();
builder.Services.AddProblemDetails();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<HolidayDbContext>();
    await dbContext.Database.EnsureCreatedAsync();
}

app.MapGet("/api/countries", async (IHolidayDataService service, bool forceSync = false) =>
{
    var countries = (await service.GetCountriesAsync(forceSync)).Select(x => new CountryDto()
    {
        countryCode = x.Code,
        name = x.Name

    });
    return Results.Ok(countries);
})
.WithName("GetCountries")
.WithOpenApi()
.WithDescription("Get all countries. Set forceSync=true to refresh from external API.");

app.MapGet("/api/holidays/last-celebrated/{countryCode}", async (
    string countryCode,
    IHolidayDataService service) =>
{
    var holidays = await service.GetLastCelebratedHolidaysAsync(countryCode);
    return Results.Ok(holidays.Select(h => new { h.Date, h.Name, h.LocalName }));
})
.WithName("GetLastCelebratedHolidays")
.WithOpenApi()
.WithDescription("Get the last three holiday celebrated in the country");

app.MapGet("/api/holidays/public-count/{year}", async (
    int year,
    IHolidayDataService service,
    [FromQuery] string[] countryCodes) =>
{
    if (countryCodes == null || countryCodes.Length == 0)
    {
        return Results.BadRequest("At least one country code must be provided.");
    }

    var result = await service.GetPublicHolidaysCountByCountryAsync(year, countryCodes.ToList());

    return Results.Ok(result.Select(kvp => new
    {
        CountryCode = kvp.Key,
        PublicHolidaysCount = kvp.Value
    }));
})
.WithName("GetPublicHolidaysCount")
.WithOpenApi()
.WithDescription("Get count of public holidays not falling on weekends for specified countries and year. Results sorted by count in descending order.");

app.MapGet("/api/holidays/shared/{year}", async (
    int year,
 string countryCode1,
    string countryCode2,
    IHolidayDataService service) =>
{
    if (string.IsNullOrWhiteSpace(countryCode1) || string.IsNullOrWhiteSpace(countryCode2))
    {
        return Results.BadRequest("Both country codes must be provided.");
    }

    if (countryCode1.Equals(countryCode2, StringComparison.OrdinalIgnoreCase))
    {
        return Results.BadRequest("Country codes must be different.");
    }

    var sharedHolidays = await service.GetSharedHolidayDatesAsync(year, countryCode1, countryCode2);

    return Results.Ok(sharedHolidays);
})
.WithName("GetSharedHolidays")
.WithOpenApi()
.WithDescription("Get deduplicated list of dates celebrated in both countries with local names for each country. Results sorted by date.");

app.MapDefaultEndpoints();

app.Run();