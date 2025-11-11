using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Accenture_Assessment.Data.Models;

namespace Accenture_Assessment.Data.Interfaces.Repositories
{
    public interface IHolidayRepository
    {
        Task<List<Holiday>> FetchHolidays();
        Task<Holiday> AddHolidayAsync(Holiday holiday);
        Task AddHolidaysAsync(IEnumerable<Holiday> holidays);

        Task<bool> HolidayExistsAsync(string countryCode, int year);
        Task<List<Holiday>> FetchHolidaysByCountryCodeAsync(string countryCode);
        Task<List<Holiday>> FetchHolidaysByCountryCodeAndYearAsync(string countryCode, int year);
        Task<List<Holiday>> FetchLastCelebratedHolidaysAsync(string countryCode, int count);
        Task<List<Holiday>> FetchPublicHolidaysByCountryCodesAndYearAsync(List<string> countryCodes, int year);
        Task<List<Holiday>> FetchHolidaysByCountryCodesAndYearAsync(List<string> countryCodes, int year);
        Task<bool> HolidayExistsAsync(string countryCode, DateTime date, string name);

    }
}
