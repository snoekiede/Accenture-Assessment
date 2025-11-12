using Accenture_Assessment.Data.Models;

namespace Accenture_Assessment.Data.Interfaces.Repositories
{
    public interface IHolidayRepository
    {
        Task<Holiday> AddHolidayAsync(Holiday holiday);
        Task AddHolidaysAsync(IEnumerable<Holiday> holidays);

        Task<List<Holiday>> FetchLastCelebratedHolidaysAsync(string countryCode, int count);
        Task<List<Holiday>> FetchPublicHolidaysByCountryCodesAndYearAsync(List<string> countryCodes, int year);
        Task<List<Holiday>> FetchHolidaysByCountryCodesAndYearAsync(List<string> countryCodes, int year);
        Task<bool> HolidayExistsAsync(string countryCode, DateTime date, string name);

    }
}
