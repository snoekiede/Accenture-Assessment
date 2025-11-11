using Accenture_Assessment.Data.Models;

namespace Accenture_Assessment.Data.Interfaces.Repositories
{
    public interface ICountryRepository
    {
        Task<List<Country>> FetchCountriesAsync();
        Task<Country> AddCountryAsync(Country country);
        Task<bool> CountryExistsAsync(string countryCode);
    }
}
