using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Accenture_Assessment.Contracts.Dtos;
using Accenture_Assessment.Data.Interfaces.Repositories;
using Accenture_Assessment.Data.Interfaces.Services;
using Accenture_Assessment.Data.Models;
using Microsoft.Extensions.Logging;

namespace Accenture_Assessment.Data.Services
{
    public class HolidayDataService : IHolidayDataService
    {
        private readonly IHolidayApiClient _apiClient;
        private readonly ICountryRepository _countryRepository;
        private readonly IHolidayRepository _holidayRepository;
        private readonly ILogger<HolidayDataService> _logger;

        public HolidayDataService(IHolidayApiClient apiClient,
            ICountryRepository countryRepository,
            IHolidayRepository holidayRepository,
            ILogger<HolidayDataService> logger)
        {
            _apiClient = apiClient;
            _countryRepository = countryRepository;
            _holidayRepository = holidayRepository;
            _logger = logger;
        }

        private static Holiday MapHolidayDtoToEntity(HolidayDto dto)
        {
            return new Holiday
            {
                CountryCode = dto.CountryCode,
                Date = dto.Date,
                Name = dto.Name,
                LocalName = dto.LocalName,
                Fixed = dto.Fixed,
                Global = dto.Global,
                Counties = dto.Counties ?? new List<string>(),
                LaunchYear = dto.LaunchYear,
                Type = dto.Type
            };
        }

        public async Task<List<Country>> SyncCountriesAsync()
        {
            _logger.LogInformation("Starting country sync from external API.");
            var countries = await _apiClient.GetCountriesAsync();
            var syncedCountries = new List<Country>();

            foreach (var country in countries)
            {
                if (await _countryRepository.CountryExistsAsync(country.countryCode))
                {
                    _logger.LogInformation("Country {CountryCode} already exists. Skipping.", country.countryCode);
                    continue;
                }

                var addedCountry = new Country
                {
                    Code = country.countryCode,
                    Name = country.name
                };
                await _countryRepository.AddCountryAsync(addedCountry);
                syncedCountries.Add(addedCountry);
                _logger.LogInformation("Added new country {CountryCode} - {CountryName}.", country.countryCode, country.name);

            }
            return syncedCountries;
        }

        public async Task<List<Country>> GetCountriesAsync(bool forceSync = false)
        {
            var countries = await _countryRepository.FetchCountriesAsync();

            if (forceSync || !countries.Any())
            {
                _logger.LogInformation("Force syncing countries from external API.");
                countries = await SyncCountriesAsync();
            }

            return countries;
        }
        public async Task<List<Holiday>> SyncLastCelebratedHolidaysAsync(string countryCode, int count = 3)
        {
            _logger.LogInformation("Syncing last {Count} celebrated holidays for country {CountryCode} from external API.", count, countryCode);
            var holidays = await _apiClient.GetLastCelebratedHolidaysAsync(countryCode, count);
            var syncedHolidays = new List<Holiday>();

            foreach (var holiday in holidays)
            {
                // Fix: Check for specific holiday, not just year
                if (await _holidayRepository.HolidayExistsAsync(countryCode, holiday.Date, holiday.Name))
                {
                    _logger.LogInformation("Holiday {HolidayName} on {HolidayDate} for country {CountryCode} already exists. Skipping.", 
                        holiday.Name, holiday.Date, countryCode);
                    continue;
                }

                var addedHoliday = MapHolidayDtoToEntity(holiday);
                await _holidayRepository.AddHolidayAsync(addedHoliday);
                syncedHolidays.Add(addedHoliday);
                _logger.LogInformation("Added new holiday {HolidayName} on {HolidayDate} for country {CountryCode}.", 
                    holiday.LocalName, holiday.Date, countryCode);
            }
            return syncedHolidays;
        }
        public async Task<List<Holiday>> GetLastCelebratedHolidaysAsync(string countryCode, int count = 3)
        {
            _logger.LogInformation("Fetching last {Count} celebrated holidays for country {CountryCode}.", count, countryCode);

            var holidays = await _holidayRepository.FetchLastCelebratedHolidaysAsync(countryCode, count);
            if (!holidays.Any())
            {
                var holidayDtos = await _apiClient.GetLastCelebratedHolidaysAsync(countryCode, count);
                holidays = holidayDtos.Select(MapHolidayDtoToEntity).ToList();
            }

            if (holidays.Count < count)
            {
                _logger.LogInformation("Not enough celebrated holidays found locally for {CountryCode}. Syncing from external API.", countryCode);
                var syncedHolidays = await SyncLastCelebratedHolidaysAsync(countryCode, count - holidays.Count);
                holidays.AddRange(syncedHolidays);
            }

            _logger.LogInformation("Found {HolidayCount} celebrated holidays for {CountryCode}.", holidays.Count, countryCode);

            return holidays;
        }

        public async Task<Dictionary<string, int>> GetPublicHolidaysCountByCountryAsync(int year, List<string> countryCodes)
        {
            _logger.LogInformation("Fetching public holidays count for year {Year} and countries: {Countries}",
                year, string.Join(", ", countryCodes));

            var holidays = await _holidayRepository.FetchPublicHolidaysByCountryCodesAndYearAsync(countryCodes, year);

            if (!holidays.Any())
            {
                _logger.LogInformation("No public holidays found locally for year {Year}. Fetching from external API.", year);
    
                // Parallel fetching for better performance
                var tasks = countryCodes.Select(code => _apiClient.GetHolidaysAsync(code, year));
                var results = await Task.WhenAll(tasks);
                var fetchedHolidays = results.SelectMany(dtos => dtos.Select(MapHolidayDtoToEntity)).ToList();

                holidays = fetchedHolidays;
                await _holidayRepository.AddHolidaysAsync(holidays);
            }

      // Filter out weekends and group by country
            var result = holidays
.Where(h => h.Date.DayOfWeek != DayOfWeek.Saturday && h.Date.DayOfWeek != DayOfWeek.Sunday)
    .GroupBy(h => h.CountryCode)
           .ToDictionary(g => g.Key, g => g.Count())
              .OrderByDescending(kvp => kvp.Value)
        .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            // Include countries with 0 holidays
       foreach (var countryCode in countryCodes)
  {
             if (!result.ContainsKey(countryCode))
      {
       result[countryCode] = 0;
     }
   }

       _logger.LogInformation("Found holidays for {CountryCount} countries.", result.Count);

            return result.OrderByDescending(kvp => kvp.Value).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }

        public async Task<List<SharedHolidayDto>> GetSharedHolidayDatesAsync(int year, string countryCode1, string countryCode2)
{
   _logger.LogInformation("Fetching shared holidays for year {Year} between {Country1} and {Country2}",
   year, countryCode1, countryCode2);

     var countryCodes = new List<string> { countryCode1, countryCode2 };
          var holidays = await _holidayRepository.FetchHolidaysByCountryCodesAndYearAsync(countryCodes, year);

            // If no holidays in database, fetch from API
      if (!holidays.Any())
       {
          _logger.LogInformation("No holidays found locally for year {Year}. Fetching from external API.", year);
                
             // Parallel fetching
      var tasks = countryCodes.Select(code => _apiClient.GetHolidaysAsync(code, year));
             var results = await Task.WhenAll(tasks);
        var fetchedHolidays = results.SelectMany(dtos => dtos.Select(MapHolidayDtoToEntity)).ToList();

        holidays = fetchedHolidays;
     await _holidayRepository.AddHolidaysAsync(holidays);
       }

  // Group by date to find shared dates
            var holidaysByDate = holidays
                .GroupBy(h => h.Date.Date)
      .Where(g => g.Select(h => h.CountryCode).Distinct().Count() == 2) // Both countries must have this date
          .ToList();

            var sharedHolidays = new List<SharedHolidayDto>();

  foreach (var dateGroup in holidaysByDate)
     {
 var country1Holiday = dateGroup.FirstOrDefault(h => h.CountryCode == countryCode1);
  var country2Holiday = dateGroup.FirstOrDefault(h => h.CountryCode == countryCode2);

    if (country1Holiday != null && country2Holiday != null)
    {
           sharedHolidays.Add(new SharedHolidayDto
        {
        Date = dateGroup.Key,
   Country1Code = countryCode1,
           Country1LocalName = country1Holiday.LocalName,
   Country2Code = countryCode2,
   Country2LocalName = country2Holiday.LocalName
             });
             }
            }

      var result = sharedHolidays.OrderBy(h => h.Date).ToList();

         _logger.LogInformation("Found {Count} shared holiday dates between {Country1} and {Country2}",
        result.Count, countryCode1, countryCode2);

  return result;
     }
    }
}