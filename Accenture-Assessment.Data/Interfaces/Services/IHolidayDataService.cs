    using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Accenture_Assessment.Data.Models;

namespace Accenture_Assessment.Data.Interfaces.Services
{
    public interface IHolidayDataService
    {
        Task<List<Country>> SyncCountriesAsync();
        Task<List<Country>> GetCountriesAsync(bool forceSync = false);
        Task<List<Holiday>> GetLastCelebratedHolidaysAsync(string countryCode, int count = 3);
        Task<Dictionary<string, int>> GetPublicHolidaysCountByCountryAsync(int year, List<string> countryCodes);
    }
}
