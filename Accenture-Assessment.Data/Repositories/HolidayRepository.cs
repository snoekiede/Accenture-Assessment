using Accenture_Assessment.Contracts.Enums;
using Accenture_Assessment.Data.Contexts;
using Accenture_Assessment.Data.Interfaces.Repositories;
using Accenture_Assessment.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Accenture_Assessment.Data.Repositories
{
    public class HolidayRepository(HolidayDbContext context) : IHolidayRepository
    {

        public async Task<Holiday> AddHolidayAsync(Holiday holiday)
        {
            // Ensure DateTime is UTC before saving to PostgreSQL
            holiday.Date = NormalizeToUtc(holiday.Date);
            
            context.Holidays.Add(holiday);
            await context.SaveChangesAsync();
            return holiday;
        }

        public async Task AddHolidaysAsync(IEnumerable<Holiday> holidays)
        {
            // Ensure all DateTime values are UTC before saving to PostgreSQL
            foreach (var holiday in holidays)
            {
                holiday.Date = NormalizeToUtc(holiday.Date);
            }
            
            context.Holidays.AddRange(holidays);
            await context.SaveChangesAsync();
        }

        private static DateTime NormalizeToUtc(DateTime dateTime)
        {
            return dateTime.Kind switch
            {
                DateTimeKind.Unspecified => DateTime.SpecifyKind(dateTime, DateTimeKind.Utc),
                DateTimeKind.Local => dateTime.ToUniversalTime(),
                DateTimeKind.Utc => dateTime,
                _ => dateTime
            };
        }

        public async Task<List<Holiday>> FetchLastCelebratedHolidaysAsync(string countryCode, int count=3)
        {
            return await context.Holidays
                .Where(h => h.CountryCode == countryCode && h.Date < DateTime.UtcNow)
                .OrderByDescending(h => h.Date)
                .Take(count)
                .ToListAsync();
        }

        public async Task<List<Holiday>> FetchPublicHolidaysByCountryCodesAndYearAsync(List<string> countryCodes, int year)
        {
            return await context.Holidays
                .Where(h => countryCodes.Contains(h.CountryCode)
                            && h.Date.Year == year
                            && h.Type == HolidayType.Public)
                .ToListAsync();
        }

        public async Task<List<Holiday>> FetchHolidaysByCountryCodesAndYearAsync(List<string> countryCodes, int year)
        {
            return await context.Holidays
                .Where(h => countryCodes.Contains(h.CountryCode) && h.Date.Year == year)
                .ToListAsync();
        }

        public async Task<bool> HolidayExistsAsync(string countryCode, DateTime date, string name)
        {
            return await context.Holidays.AnyAsync(h =>
                h.CountryCode == countryCode &&
                h.Date.Date == date.Date &&
                h.Name == name);
        }
    }
}
