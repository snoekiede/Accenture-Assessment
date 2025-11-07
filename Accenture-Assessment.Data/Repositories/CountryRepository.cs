using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Accenture_Assessment.Data.Contexts;
using Accenture_Assessment.Data.Interfaces.Repositories;
using Accenture_Assessment.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Accenture_Assessment.Data.Repositories
{
    public class CountryRepository: ICountryRepository
    {
        private readonly HolidayDbContext _dbContext;

        public CountryRepository(HolidayDbContext dbContext)
        {
            _dbContext = dbContext;
        }


        public async Task<List<Country>> FetchCountriesAsync()
        {
            return await _dbContext.Countries.OrderBy(c => c.Name).ToListAsync();
        }

        public async Task<Country> AddCountryAsync(Country country)
        {
            _dbContext.Countries.Add(country);
            await _dbContext.SaveChangesAsync();
            return country;
        }

        public async Task<bool> CountryExistsAsync(string countryCode)
        {
            return await _dbContext.Countries.AnyAsync(c => c.Code == countryCode);
        }
    }
}
