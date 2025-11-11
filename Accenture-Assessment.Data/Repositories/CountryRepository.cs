using Accenture_Assessment.Data.Contexts;
using Accenture_Assessment.Data.Interfaces.Repositories;
using Accenture_Assessment.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Accenture_Assessment.Data.Repositories
{
    public class CountryRepository(HolidayDbContext dbContext) : ICountryRepository
    {
        public async Task<List<Country>> FetchCountriesAsync()
        {
            return await dbContext.Countries.OrderBy(c => c.Name).ToListAsync();
        }

        public async Task<Country> AddCountryAsync(Country country)
        {
            dbContext.Countries.Add(country);
            await dbContext.SaveChangesAsync();
            return country;
        }

        public async Task<bool> CountryExistsAsync(string countryCode)
        {
            return await dbContext.Countries.AnyAsync(c => c.Code == countryCode);
        }
    }
}
