using Accenture_Assessment.Contracts.Enums;
using Accenture_Assessment.Data.Contexts;
using Accenture_Assessment.Data.Interfaces.Repositories;
using Accenture_Assessment.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accenture_Assessment.Data.Repositories
{
    public class HolidayRepository:IHolidayRepository
    {
        private readonly HolidayDbContext _context;

        public HolidayRepository(HolidayDbContext context)
        {
            _context = context;
        }

        public async Task<List<Holiday>> FetchHolidays()
        {
            return await _context.Holidays.ToListAsync();
        }

        public async Task<Holiday> AddHolidayAsync(Holiday holiday)
        {
            _context.Holidays.Add(holiday);
            await _context.SaveChangesAsync();
            return holiday;
        }

        public async Task AddHolidaysAsync(IEnumerable<Holiday> holidays)
        {
            _context.Holidays.AddRange(holidays);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> HolidayExistsAsync(string countryCode, int year)
        {
           return await _context.Holidays.AnyAsync(x=>x.CountryCode==countryCode && x.Date.Year==year);
        }

        public async Task<List<Holiday>> FetchHolidaysByCountryCodeAsync(string countryCode)
        {
            return await _context.Holidays.Where(x => x.CountryCode == countryCode).ToListAsync();
        }

        public async Task<List<Holiday>> FetchHolidaysByCountryCodeAndYearAsync(string countryCode, int year)
        {
            return await _context.Holidays.Where(x => x.CountryCode == countryCode && x.Date.Year == year).ToListAsync();
        }

        public async Task<List<Holiday>> FetchLastCelebratedHolidaysAsync(string countryCode, int count=3)
        {
            return await _context.Holidays
                .Where(h => h.CountryCode == countryCode && h.Date < DateTime.Now)
                .OrderByDescending(h => h.Date)
                .Take(count)
                .ToListAsync();
        }

        public async Task<List<Holiday>> FetchPublicHolidaysByCountryCodesAndYearAsync(List<string> countryCodes, int year)
        {
            return await _context.Holidays
                .Where(h => countryCodes.Contains(h.CountryCode)
                            && h.Date.Year == year
                            && h.Type == HolidayType.Public)
                .ToListAsync();
        }

        public async Task<List<Holiday>> FetchHolidaysByCountryCodesAndYearAsync(List<string> countryCodes, int year)
        {
            return await _context.Holidays
                .Where(h => countryCodes.Contains(h.CountryCode) && h.Date.Year == year)
                .ToListAsync();
        }
    }
}
