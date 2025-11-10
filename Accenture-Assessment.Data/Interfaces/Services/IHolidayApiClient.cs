using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Accenture_Assessment.Contracts.Dtos;
using Accenture_Assessment.Data.Models;

namespace Accenture_Assessment.Data.Interfaces.Services
{
    public interface IHolidayApiClient
    {
        Task<List<CountryDto>> GetCountriesAsync();
        Task<List<HolidayDto>> GetHolidaysAsync(string countryCode, int year);
        Task<List<HolidayDto>> GetLastCelebratedHolidaysAsync(string countryCode, int count);
    }
}
