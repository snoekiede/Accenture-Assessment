using Accenture_Assessment.Contracts.Dtos;
using Accenture_Assessment.Contracts.Enums;
using Accenture_Assessment.Data.Interfaces.Repositories;
using Accenture_Assessment.Data.Interfaces.Services;
using Accenture_Assessment.Data.Models;
using Accenture_Assessment.Data.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace Accenture_Assessment.Tests;

[TestFixture]
public class HolidayDataServiceTests
{
    private Mock<IHolidayApiClient> _mockApiClient = null!;
    private Mock<ICountryRepository> _mockCountryRepo = null!;
    private Mock<IHolidayRepository> _mockHolidayRepo = null!;
    private Mock<ILogger<HolidayDataService>> _mockLogger = null!;
    private HolidayDataService _service = null!;

    [SetUp]
    public void Setup()
    {
        _mockApiClient = new Mock<IHolidayApiClient>();
        _mockCountryRepo = new Mock<ICountryRepository>();
        _mockHolidayRepo = new Mock<IHolidayRepository>();
        _mockLogger = new Mock<ILogger<HolidayDataService>>();
        
        _service = new HolidayDataService(
            _mockApiClient.Object,
            _mockCountryRepo.Object,
            _mockHolidayRepo.Object,
            _mockLogger.Object);
    }

    [Test]
    public async Task SyncCountriesAsync_SkipsExistingCountries()
    {
        // Arrange
        var apiCountries = new List<CountryDto>
        {
            new CountryDto { countryCode = "US", name = "United States" },
            new CountryDto { countryCode = "CA", name = "Canada" }
        };
        
        _mockApiClient.Setup(x => x.GetCountriesAsync())
            .ReturnsAsync(apiCountries);
        
        // US already exists, CA doesn't
        _mockCountryRepo.Setup(x => x.CountryExistsAsync("US")).ReturnsAsync(true);
        _mockCountryRepo.Setup(x => x.CountryExistsAsync("CA")).ReturnsAsync(false);
        
        // Act
        var result = await _service.SyncCountriesAsync();
        
        // Assert - Only CA should be added
        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result[0].Code, Is.EqualTo("CA"));
        
        _mockCountryRepo.Verify(x => x.AddCountryAsync(It.Is<Country>(c => c.Code == "CA")), Times.Once);
        _mockCountryRepo.Verify(x => x.AddCountryAsync(It.Is<Country>(c => c.Code == "US")), Times.Never);
    }

    [Test]
    public async Task GetLastCelebratedHolidaysAsync_FallsBackToApi_WhenDatabaseEmpty()
    {
        // Arrange
        var countryCode = "US";
        var now = DateTime.Now;
        
        // Set up 3 holidays so the service doesn't need to sync more
        var apiHolidays = new List<HolidayDto>
        {
            new HolidayDto 
            { 
                CountryCode = "US", 
                Date = now.AddDays(-10), 
                Name = "Holiday 1", 
                LocalName = "Holiday 1",
                Type = HolidayType.Public
            },
            new HolidayDto 
            { 
                CountryCode = "US", 
                Date = now.AddDays(-20), 
                Name = "Holiday 2", 
                LocalName = "Holiday 2",
                Type = HolidayType.Public
            },
            new HolidayDto 
            { 
                CountryCode = "US", 
                Date = now.AddDays(-30), 
                Name = "Holiday 3", 
                LocalName = "Holiday 3",
                Type = HolidayType.Public
            }
        };
        
        // Database returns empty
        _mockHolidayRepo.Setup(x => x.FetchLastCelebratedHolidaysAsync(countryCode, 3))
            .ReturnsAsync(new List<Holiday>());
        
        // API returns data (will be called once)
        _mockApiClient.Setup(x => x.GetLastCelebratedHolidaysAsync(countryCode, It.IsAny<int>()))
            .ReturnsAsync(apiHolidays);
        
        // Mock HolidayExistsAsync for the sync operation (in case it's called)
        _mockHolidayRepo.Setup(x => x.HolidayExistsAsync(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<string>()))
            .ReturnsAsync(false);
        
        // Mock AddHolidayAsync to return the holiday
        _mockHolidayRepo.Setup(x => x.AddHolidayAsync(It.IsAny<Holiday>()))
            .ReturnsAsync((Holiday h) => { h.Id = 1; return h; });
        
        // Act
        var result = await _service.GetLastCelebratedHolidaysAsync(countryCode, 3);
        
        // Assert
        Assert.That(result, Has.Count.EqualTo(3));
        Assert.That(result[0].Name, Is.EqualTo("Holiday 1"));
        Assert.That(result[1].Name, Is.EqualTo("Holiday 2"));
        Assert.That(result[2].Name, Is.EqualTo("Holiday 3"));
        
        // Verify API was called because DB was empty
        _mockApiClient.Verify(x => x.GetLastCelebratedHolidaysAsync(countryCode, It.IsAny<int>()), Times.AtLeastOnce);
    }

    [Test]
    public async Task GetPublicHolidaysCountByCountryAsync_ExcludesWeekends()
    {
        // Arrange
        var holidays = new List<Holiday>
        {
            // Saturday - should be excluded
            new Holiday 
            { 
                CountryCode = "US", 
                Date = new DateTime(2024, 12, 21), 
                Name = "Weekend", 
                LocalName = "Weekend",
                Type = HolidayType.Public 
            },
            // Sunday - should be excluded
            new Holiday 
            { 
                CountryCode = "US", 
                Date = new DateTime(2024, 12, 22), 
                Name = "Weekend", 
                LocalName = "Weekend",
                Type = HolidayType.Public 
            },
            // Wednesday - should be counted
            new Holiday 
            { 
                CountryCode = "US", 
                Date = new DateTime(2024, 12, 25), 
                Name = "Christmas", 
                LocalName = "Christmas",
                Type = HolidayType.Public 
            },
            // Thursday - should be counted
            new Holiday 
            { 
                CountryCode = "US", 
                Date = new DateTime(2024, 7, 4), 
                Name = "Independence", 
                LocalName = "Independence",
                Type = HolidayType.Public 
            }
        };
        
        _mockHolidayRepo.Setup(x => x.FetchPublicHolidaysByCountryCodesAndYearAsync(
            It.IsAny<List<string>>(), 2024))
            .ReturnsAsync(holidays);
        
        // Act
        var result = await _service.GetPublicHolidaysCountByCountryAsync(2024, new List<string> { "US" });
        
        // Assert - Only weekday holidays counted
        Assert.That(result["US"], Is.EqualTo(2));
    }

    [Test]
    public async Task GetPublicHolidaysCountByCountryAsync_SortsDescending()
    {
        // Arrange
        var holidays = new List<Holiday>
        {
            new Holiday { CountryCode = "US", Date = new DateTime(2024, 1, 1), Name = "NY", LocalName = "NY", Type = HolidayType.Public },
            new Holiday { CountryCode = "US", Date = new DateTime(2024, 7, 4), Name = "July4", LocalName = "July4", Type = HolidayType.Public },
            new Holiday { CountryCode = "CA", Date = new DateTime(2024, 1, 1), Name = "NY", LocalName = "NY", Type = HolidayType.Public },
            new Holiday { CountryCode = "GB", Date = new DateTime(2024, 12, 25), Name = "Xmas", LocalName = "Xmas", Type = HolidayType.Public },
            new Holiday { CountryCode = "GB", Date = new DateTime(2024, 1, 1), Name = "NY", LocalName = "NY", Type = HolidayType.Public },
            new Holiday { CountryCode = "GB", Date = new DateTime(2024, 12, 26), Name = "Boxing", LocalName = "Boxing", Type = HolidayType.Public }
        };
        
        _mockHolidayRepo.Setup(x => x.FetchPublicHolidaysByCountryCodesAndYearAsync(
            It.IsAny<List<string>>(), 2024))
            .ReturnsAsync(holidays);
        
        // Act
        var result = await _service.GetPublicHolidaysCountByCountryAsync(2024, 
            new List<string> { "US", "CA", "GB" });
        
        // Assert - Sorted descending: GB(3), US(2), CA(1)
        var resultList = result.ToList();
        Assert.That(resultList[0].Key, Is.EqualTo("GB"));
        Assert.That(resultList[0].Value, Is.EqualTo(3));
        Assert.That(resultList[1].Key, Is.EqualTo("US"));
        Assert.That(resultList[1].Value, Is.EqualTo(2));
        Assert.That(resultList[2].Key, Is.EqualTo("CA"));
        Assert.That(resultList[2].Value, Is.EqualTo(1));
    }

    [Test]
    public async Task GetSharedHolidayDatesAsync_IdentifiesSharedDates()
    {
        // Arrange
        var holidays = new List<Holiday>
        {
            // Shared date
            new Holiday { CountryCode = "US", Date = new DateTime(2024, 12, 25), Name = "Christmas", LocalName = "Christmas Day" },
            new Holiday { CountryCode = "CA", Date = new DateTime(2024, 12, 25), Name = "Christmas", LocalName = "Noël" },
            
            // Not shared (only US)
            new Holiday { CountryCode = "US", Date = new DateTime(2024, 7, 4), Name = "Independence", LocalName = "Independence Day" },
            
            // Not shared (only CA)
            new Holiday { CountryCode = "CA", Date = new DateTime(2024, 7, 1), Name = "Canada Day", LocalName = "Fête du Canada" }
        };
        
        _mockHolidayRepo.Setup(x => x.FetchHolidaysByCountryCodesAndYearAsync(
            It.Is<List<string>>(l => l.Contains("US") && l.Contains("CA")), 2024))
            .ReturnsAsync(holidays);
        
        // Act
        var result = await _service.GetSharedHolidayDatesAsync(2024, "US", "CA");
        
        // Assert - Only Christmas is shared
        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result[0].Date, Is.EqualTo(new DateTime(2024, 12, 25)));
        Assert.That(result[0].Country1LocalName, Is.EqualTo("Christmas Day"));
        Assert.That(result[0].Country2LocalName, Is.EqualTo("Noël"));
    }

    [Test]
    public async Task SyncLastCelebratedHolidaysAsync_SkipsDuplicates()
    {
        // Arrange
        var countryCode = "US";
        var date = new DateTime(2024, 12, 25);
        var name = "Christmas";
        
        var apiHolidays = new List<HolidayDto>
        {
            new HolidayDto 
            { 
                CountryCode = countryCode, 
                Date = date, 
                Name = name, 
                LocalName = name,
                Type = HolidayType.Public
            }
        };
        
        _mockApiClient.Setup(x => x.GetLastCelebratedHolidaysAsync(countryCode, 3))
            .ReturnsAsync(apiHolidays);
        
        // Holiday already exists
        _mockHolidayRepo.Setup(x => x.HolidayExistsAsync(countryCode, date, name))
            .ReturnsAsync(true);
        
        // Act
        var result = await _service.SyncLastCelebratedHolidaysAsync(countryCode, 3);
        
        // Assert - No holidays added (all skipped)
        Assert.That(result, Is.Empty);
        _mockHolidayRepo.Verify(x => x.AddHolidayAsync(It.IsAny<Holiday>()), Times.Never);
    }
}