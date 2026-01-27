using System.Net.Http.Json;
using Accenture_Assessment.Contracts.Dtos;
using Accenture_Assessment.Data.Exceptions;
using Accenture_Assessment.Data.Interfaces.Services;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Accenture_Assessment.Data.Services
{
    public class HolidayApiClient(HttpClient httpClient, ILogger<HolidayApiClient> logger) : IHolidayApiClient
    {
        private const int MaxYearLookback = 10;
        private const int MinYear = 1900;

        public async Task<List<CountryDto>> GetCountriesAsync()
        {
            try
            {
                logger.LogInformation("Fetching available countries from external API");

                var response = await httpClient.GetAsync("AvailableCountries");
                response.EnsureSuccessStatusCode();

                var countries = await response.Content.ReadFromJsonAsync<List<CountryDto>>()
                    ?? throw new InvalidOperationException("API returned null countries list");

                logger.LogInformation("Successfully fetched {Count} countries", countries.Count);
                return countries;
            }
            catch (HttpRequestException ex)
            {
                logger.LogError(ex, "HTTP error fetching countries: StatusCode={StatusCode}", ex.StatusCode);
                throw new ExternalApiException($"External API HTTP error: {ex.StatusCode}", ex);
            }
            catch (JsonException ex)
            {
                logger.LogError(ex, "JSON deserialization error for countries");
                throw new ExternalApiException("Invalid API response format for countries", ex);
            }
            catch (TaskCanceledException ex)
            {
                logger.LogError(ex, "Request timeout fetching countries");
                throw new ExternalApiException("API request timeout for countries", ex);
            }
        }

        public async Task<List<HolidayDto>> GetHolidaysAsync(string countryCode, int year)
        {
            try
            {
                logger.LogInformation("Fetching holidays for {CountryCode} in {Year}", countryCode, year);

                var response = await httpClient.GetAsync($"publicholidays/{year}/{countryCode}");
                response.EnsureSuccessStatusCode();

                var holidays = await response.Content.ReadFromJsonAsync<List<HolidayDto>>()
                    ?? throw new InvalidOperationException($"API returned null holidays for {countryCode}/{year}");

                logger.LogInformation("Successfully fetched {Count} holidays for {CountryCode} in {Year}",
                    holidays.Count, countryCode, year);
                return holidays;
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                logger.LogWarning("No holidays found for {CountryCode} in {Year}", countryCode, year);
                return []; // Return empty list for 404, not an error
            }
            catch (HttpRequestException ex)
            {
                logger.LogError(ex, "HTTP error fetching holidays for {CountryCode} in {Year}: StatusCode={StatusCode}",
                    countryCode, year, ex.StatusCode);
                throw new ExternalApiException($"External API HTTP error for {countryCode}/{year}: {ex.StatusCode}", ex);
            }
            catch (JsonException ex)
            {
                logger.LogError(ex, "JSON deserialization error for holidays {CountryCode}/{Year}", countryCode, year);
                throw new ExternalApiException($"Invalid API response format for {countryCode}/{year}", ex);
            }
            catch (TaskCanceledException ex)
            {
                logger.LogError(ex, "Request timeout fetching holidays for {CountryCode}/{Year}", countryCode, year);
                throw new ExternalApiException($"API request timeout for {countryCode}/{year}", ex);
            }
        }

        public async Task<List<HolidayDto>> GetLastCelebratedHolidaysAsync(string countryCode, int count)
        {
            var celebratedHolidays = new List<HolidayDto>();
            var currentDate = DateTime.UtcNow;
            var currentYear = currentDate.Year;
            var yearOffset = 0;

            try
            {
                logger.LogInformation("Fetching last {Count} celebrated holidays for {CountryCode}", count, countryCode);

                // Search through previous years until we find enough celebrated holidays
                while (celebratedHolidays.Count < count && yearOffset < MaxYearLookback)
                {
                    var year = currentYear - yearOffset;

                    // Stop if we've gone back too far
                    if (year < MinYear)
                    {
                        logger.LogWarning("Reached minimum year {MinYear} while searching for holidays in {CountryCode}",
                            MinYear, countryCode);
                        break;
                    }

                    var holidays = await GetHolidaysAsync(countryCode, year);

                    var pastHolidays = holidays
                        .Where(h => h.Date < currentDate)
                        .OrderByDescending(h => h.Date)
                        .Take(count - celebratedHolidays.Count)
                        .ToList();

                    celebratedHolidays.AddRange(pastHolidays);
                    yearOffset++;

                    // If we haven't found any holidays in the last 2 years, likely no more exist
                    if (celebratedHolidays.Count == 0 && yearOffset >= 2)
                    {
                        logger.LogWarning("No celebrated holidays found for {CountryCode} after searching {Years} years",
                            countryCode, yearOffset);
                        break;
                    }
                }

                var result = celebratedHolidays
                    .OrderByDescending(h => h.Date)
                    .Take(count)
                    .ToList();

                logger.LogInformation("Successfully fetched {Count} celebrated holidays for {CountryCode} (requested {Requested})",
                    result.Count, countryCode, count);

                return result;
            }
            catch (ExternalApiException ex)
            {
                logger.LogWarning(ex, "Partial failure fetching last celebrated holidays for {CountryCode}, returning {Count} results",
                    countryCode, celebratedHolidays.Count);

                // If we have no data at all, re-throw the exception
                if (celebratedHolidays.Count == 0)
                {
                    throw;
                }

                // Return partial results
                return celebratedHolidays
                    .OrderByDescending(h => h.Date)
                    .Take(count)
                    .ToList();
            }
        }
    }
}