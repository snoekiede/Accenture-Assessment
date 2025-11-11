using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Accenture_Assessment.Contracts.Dtos;
using Accenture_Assessment.Data.Interfaces.Services;
using Accenture_Assessment.Data.Models;
using Microsoft.Extensions.Logging;

namespace Accenture_Assessment.Data.Services
{
    public class HolidayApiClient : IHolidayApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<HolidayApiClient> _logger;

        public HolidayApiClient(HttpClient httpClient, ILogger<HolidayApiClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<List<CountryDto>> GetCountriesAsync()
        {
            try
            {
                _logger.LogInformation("Fetching available countries from external API");

                var response = await _httpClient.GetAsync("AvailableCountries");
                response.EnsureSuccessStatusCode();

                var countries = response.Content.ReadFromJsonAsync<List<CountryDto>>().Result;
                return countries ?? new List<CountryDto>();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error fetching countries from external API: {Message}", e.Message);
                throw;
            }
        }

        public async Task<List<HolidayDto>> GetHolidaysAsync(string countryCode, int year)
        {
            try
            {
                _logger.LogInformation("Fetching holidays for {countryCode} in {Year}", countryCode, year);
                var response = await _httpClient.GetAsync($"publicholidays/{year}/{countryCode}");
                response.EnsureSuccessStatusCode();

                var holidays = await response.Content.ReadFromJsonAsync<List<HolidayDto>>();
                return holidays ?? new List<HolidayDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching holidays for {countryCode} in {Year}", countryCode, year);
                throw;
            }
        }
        public async Task<List<PublicHolidayCountDto>> GetPublicHolidayCountsAsync(List<string> countryCodes, int year)
        {
            try
            {
                _logger.LogInformation("Fetching public holiday counts for multiple countries in {Year}", year);
                var countryCodesParam = string.Join(",", countryCodes);
                var response = await _httpClient.GetAsync($"publicholidays/counts/{year}?countryCodes={countryCodesParam}");
                response.EnsureSuccessStatusCode();
                var holidayCounts = await response.Content.ReadFromJsonAsync<List<PublicHolidayCountDto>>();
                return holidayCounts ?? new List<PublicHolidayCountDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching public holiday counts for multiple countries in {Year}: {Message}",
                    year, ex.Message);
                throw;
            }
        }
        public async Task<List<HolidayDto>> GetLastCelebratedHolidaysAsync(string countryCode, int count)
        {
            var celebratedHolidays = new List<HolidayDto>();
            try
            {
                _logger.LogInformation("Fetching last {Count} celebrated holidays for {CountryCode}", count,
                    countryCode);

                var currentDate = DateTime.Now;

                var currentYear = currentDate.Year;
                var yearOffset = 0;


                // Search through previous years until we find enough celebrated holidays
                while (celebratedHolidays.Count < count)
                {
                    var year = currentYear - yearOffset;
                    var holidays = await GetHolidaysAsync(countryCode, year);

                    var pastHolidays = holidays
                        .Where(h => h.Date < currentDate)
                        .OrderByDescending(h => h.Date)
                        .Take(count - celebratedHolidays.Count)
                        .ToList();

                    celebratedHolidays.AddRange(pastHolidays);
                    yearOffset++;
                }

                // Return exactly the count requested, sorted by most recent
                return celebratedHolidays
                    .OrderByDescending(h => h.Date)
                    .Take(count)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching last celebrated holidays for {CountryCode}: {Message}",
                    countryCode, ex.Message);
                // apparently some error, or the year was no long available
                return celebratedHolidays
                    .OrderByDescending(h => h.Date)
                    .Take(count)
                    .ToList();
            }
        }
    }
}
