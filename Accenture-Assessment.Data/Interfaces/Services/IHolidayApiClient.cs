using Accenture_Assessment.Contracts.Dtos;

namespace Accenture_Assessment.Data.Interfaces.Services
{
    public interface IHolidayApiClient
    {
        Task<List<CountryDto>> GetCountriesAsync();
        Task<List<HolidayDto>> GetHolidaysAsync(string countryCode, int year);
        Task<List<HolidayDto>> GetLastCelebratedHolidaysAsync(string countryCode, int count);
    }
}
