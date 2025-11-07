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
                _logger.LogInformation("Fetching holidays for {countryCode} in {Year}",countryCode,year);
                var response =  await _httpClient.GetAsync($"publicholidays/{year}/{countryCode}");
                response.EnsureSuccessStatusCode();

                var holidays = await response.Content.ReadFromJsonAsync<List<HolidayDto>>();
                return holidays ?? new List<HolidayDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,"Error fetching holidays for {countryCode} in {Year}",countryCode,year);
                throw;
            }
        }
    }
}
