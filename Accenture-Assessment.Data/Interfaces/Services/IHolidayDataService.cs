using Accenture_Assessment.Contracts.Dtos;
using Accenture_Assessment.Data.Models;

namespace Accenture_Assessment.Data.Interfaces.Services
{
    public interface IHolidayDataService
    {
        Task<List<Country>> GetCountriesAsync(bool forceSync = false);
        Task<List<Holiday>> GetLastCelebratedHolidaysAsync(string countryCode, int count = 3);
        Task<Dictionary<string, int>> GetPublicHolidaysCountByCountryAsync(int year, List<string> countryCodes);
        Task<List<SharedHolidayDto>> GetSharedHolidayDatesAsync(int year, string countryCode1, string countryCode2);
    }
}
