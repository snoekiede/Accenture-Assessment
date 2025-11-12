using Accenture_Assessment.Contracts.Enums;
using Accenture_Assessment.Data.Contexts;
using Accenture_Assessment.Data.Models;
using Accenture_Assessment.Data.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Accenture_Assessment.Tests;

[TestFixture]
public class HolidayRepositoryTests
{
    private HolidayDbContext _dbContext = null!;
    private HolidayRepository _repository = null!;

    [SetUp]
    public void Setup()
    {
        // Create a new in-memory database for each test
        var options = new DbContextOptionsBuilder<HolidayDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new HolidayDbContext(options);
        _repository = new HolidayRepository(_dbContext);
    }

    [TearDown]
    public void TearDown()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }

    #region AddHolidayAsync Tests

    [Test]
    public async Task AddHolidayAsync_AddsHolidaySuccessfully()
    {
        // Arrange
        var holiday = new Holiday
        {
            CountryCode = "US",
            Date = new DateTime(2024, 12, 25),
            Name = "Christmas Day",
            LocalName = "Christmas Day",
            Type = HolidayType.Public,
            Fixed = true,
            Global = true
        };

        // Act
        var result = await _repository.AddHolidayAsync(holiday);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.GreaterThan(0));
        Assert.That(result.CountryCode, Is.EqualTo("US"));
        Assert.That(result.Name, Is.EqualTo("Christmas Day"));

        // Verify it's in the database
        var savedHoliday = await _dbContext.Holidays.FirstOrDefaultAsync(h => h.Name == "Christmas Day");
        Assert.That(savedHoliday, Is.Not.Null);
        Assert.That(savedHoliday!.CountryCode, Is.EqualTo("US"));
    }

    [Test]
    public async Task AddHolidayAsync_AssignsId()
    {
        // Arrange
        var holiday = new Holiday
        {
            CountryCode = "US",
            Date = new DateTime(2024, 7, 4),
            Name = "Independence Day",
            LocalName = "Independence Day",
            Type = HolidayType.Public
        };

        // Act
        var result = await _repository.AddHolidayAsync(holiday);

        // Assert
        Assert.That(result.Id, Is.GreaterThan(0), "Id should be assigned after saving");
    }

    [Test]
    public async Task AddHolidayAsync_PreservesAllProperties()
    {
        // Arrange
        var holiday = new Holiday
        {
            CountryCode = "US",
            Date = new DateTime(2024, 7, 4),
            Name = "Independence Day",
            LocalName = "Fourth of July",
            Type = HolidayType.Public,
            Fixed = true,
            Global = true,
            Counties = ["County1", "County2"],
            LaunchYear = 1776
        };

        // Act
        var result = await _repository.AddHolidayAsync(holiday);

        // Assert
        Assert.That(result.CountryCode, Is.EqualTo("US"));
        Assert.That(result.Name, Is.EqualTo("Independence Day"));
        Assert.That(result.LocalName, Is.EqualTo("Fourth of July"));
        Assert.That(result.Fixed, Is.True);
        Assert.That(result.Global, Is.True);
        Assert.That(result.Counties, Has.Count.EqualTo(2));
        Assert.That(result.LaunchYear, Is.EqualTo(1776));
    }

    #endregion

    #region AddHolidaysAsync Tests

    [Test]
    public async Task AddHolidaysAsync_AddsMultipleHolidays()
    {
        // Arrange
        var holidays = new List<Holiday>
        {
            new()
            {
                CountryCode = "US",
                Date = new DateTime(2024, 12, 25),
                Name = "Christmas Day",
                LocalName = "Christmas Day",
                Type = HolidayType.Public
            },
            new()
            {
                CountryCode = "US",
                Date = new DateTime(2024, 7, 4),
                Name = "Independence Day",
                LocalName = "Independence Day",
                Type = HolidayType.Public
            },
            new()
            {
                CountryCode = "US",
                Date = new DateTime(2024, 1, 1),
                Name = "New Year's Day",
                LocalName = "New Year's Day",
                Type = HolidayType.Public
            }
        };

        // Act
        await _repository.AddHolidaysAsync(holidays);

        // Assert
        var savedHolidays = await _dbContext.Holidays.ToListAsync();
        Assert.That(savedHolidays, Has.Count.EqualTo(3));
        Assert.That(savedHolidays.All(h => h.Id > 0), Is.True, "All holidays should have assigned IDs");
    }

    [Test]
    public async Task AddHolidaysAsync_EmptyList_DoesNotThrow()
    {
        // Arrange
        var holidays = new List<Holiday>();

        // Act & Assert
        Assert.DoesNotThrowAsync(async () => await _repository.AddHolidaysAsync(holidays));
        
        var savedHolidays = await _dbContext.Holidays.ToListAsync();
        Assert.That(savedHolidays, Is.Empty);
    }

    #endregion

    #region FetchLastCelebratedHolidaysAsync Tests

    [Test]
    public async Task FetchLastCelebratedHolidaysAsync_ReturnsLastThreeHolidays()
    {
        // Arrange
        var now = DateTime.Now;
        var holidays = new List<Holiday>
        {
            CreateHoliday("US", now.AddDays(-10), "Holiday 1"),
            CreateHoliday("US", now.AddDays(-20), "Holiday 2"),
            CreateHoliday("US", now.AddDays(-30), "Holiday 3"),
            CreateHoliday("US", now.AddDays(-40), "Holiday 4"),
            CreateHoliday("US", now.AddDays(-50), "Holiday 5")
        };

        await _dbContext.Holidays.AddRangeAsync(holidays);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _repository.FetchLastCelebratedHolidaysAsync("US");

        // Assert
        Assert.That(result, Has.Count.EqualTo(3));
        Assert.That(result[0].Name, Is.EqualTo("Holiday 1"), "Should be most recent");
        Assert.That(result[1].Name, Is.EqualTo("Holiday 2"));
        Assert.That(result[2].Name, Is.EqualTo("Holiday 3"));
    }

    [Test]
    public async Task FetchLastCelebratedHolidaysAsync_ExcludesFutureHolidays()
    {
        // Arrange
        var now = DateTime.Now;
        var holidays = new List<Holiday>
        {
            CreateHoliday("US", now.AddDays(-10), "Past Holiday 1"),
            CreateHoliday("US", now.AddDays(10), "Future Holiday"),
            CreateHoliday("US", now.AddDays(-20), "Past Holiday 2")
        };

        await _dbContext.Holidays.AddRangeAsync(holidays);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _repository.FetchLastCelebratedHolidaysAsync("US");

        // Assert
        Assert.That(result, Has.Count.EqualTo(2));
        Assert.That(result.All(h => h.Date < now), Is.True, "Should only return past holidays");
        Assert.That(result.Any(h => h.Name == "Future Holiday"), Is.False);
    }

    [Test]
    public async Task FetchLastCelebratedHolidaysAsync_FiltersByCountryCode()
    {
        // Arrange
        var now = DateTime.Now;
        var holidays = new List<Holiday>
        {
            CreateHoliday("US", now.AddDays(-10), "US Holiday 1"),
            CreateHoliday("CA", now.AddDays(-10), "CA Holiday 1"),
            CreateHoliday("US", now.AddDays(-20), "US Holiday 2"),
            CreateHoliday("CA", now.AddDays(-20), "CA Holiday 2")
        };

        await _dbContext.Holidays.AddRangeAsync(holidays);
        await _dbContext.SaveChangesAsync();

        // Act
        var usHolidays = await _repository.FetchLastCelebratedHolidaysAsync("US");
        var caHolidays = await _repository.FetchLastCelebratedHolidaysAsync("CA");

        // Assert
        Assert.That(usHolidays, Has.Count.EqualTo(2));
        Assert.That(usHolidays.All(h => h.CountryCode == "US"), Is.True);
        
        Assert.That(caHolidays, Has.Count.EqualTo(2));
        Assert.That(caHolidays.All(h => h.CountryCode == "CA"), Is.True);
    }

    [Test]
    public async Task FetchLastCelebratedHolidaysAsync_ReturnsEmptyForNonExistentCountry()
    {
        // Arrange
        var now = DateTime.Now;
        var holidays = new List<Holiday>
        {
            CreateHoliday("US", now.AddDays(-10), "US Holiday")
        };

        await _dbContext.Holidays.AddRangeAsync(holidays);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _repository.FetchLastCelebratedHolidaysAsync("GB");

        // Assert
        Assert.That(result, Is.Empty);
    }

    #endregion

    #region FetchPublicHolidaysByCountryCodesAndYearAsync Tests

    [Test]
    public async Task FetchPublicHolidaysByCountryCodesAndYearAsync_ReturnsOnlyPublicHolidays()
    {
        // Arrange
        var holidays = new List<Holiday>
        {
            CreateHoliday("US", new DateTime(2024, 12, 25), "Christmas"),
            CreateHoliday("US", new DateTime(2024, 1, 1), "New Year"),
            CreateHoliday("US", new DateTime(2024, 2, 14), "Valentine's Day", HolidayType.Observance),
            CreateHoliday("US", new DateTime(2024, 10, 31), "Halloween", HolidayType.Observance)
        };

        await _dbContext.Holidays.AddRangeAsync(holidays);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _repository.FetchPublicHolidaysByCountryCodesAndYearAsync(["US"], 2024);

        // Assert
        Assert.That(result, Has.Count.EqualTo(2));
        Assert.That(result.All(h => h.Type == HolidayType.Public), Is.True);
    }

    [Test]
    public async Task FetchPublicHolidaysByCountryCodesAndYearAsync_FiltersByYear()
    {
        // Arrange
        var holidays = new List<Holiday>
        {
            CreateHoliday("US", new DateTime(2024, 12, 25), "Christmas 2024"),
            CreateHoliday("US", new DateTime(2023, 12, 25), "Christmas 2023"),
            CreateHoliday("US", new DateTime(2025, 12, 25), "Christmas 2025")
        };

        await _dbContext.Holidays.AddRangeAsync(holidays);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _repository.FetchPublicHolidaysByCountryCodesAndYearAsync(["US"], 2024);

        // Assert
        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result[0].Name, Is.EqualTo("Christmas 2024"));
    }

    [Test]
    public async Task FetchPublicHolidaysByCountryCodesAndYearAsync_HandlesMultipleCountries()
    {
        // Arrange
        var holidays = new List<Holiday>
        {
            CreateHoliday("US", new DateTime(2024, 12, 25), "US Christmas"),
            CreateHoliday("CA", new DateTime(2024, 12, 25), "CA Christmas"),
            CreateHoliday("GB", new DateTime(2024, 12, 25), "GB Christmas"),
            CreateHoliday("FR", new DateTime(2024, 12, 25), "FR Christmas")
        };

        await _dbContext.Holidays.AddRangeAsync(holidays);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _repository.FetchPublicHolidaysByCountryCodesAndYearAsync(
            ["US", "CA", "GB"], 2024);

        // Assert
        Assert.That(result, Has.Count.EqualTo(3));
        Assert.That(result.Any(h => h.CountryCode == "FR"), Is.False);
        Assert.That(result.Any(h => h.CountryCode == "US"), Is.True);
        Assert.That(result.Any(h => h.CountryCode == "CA"), Is.True);
        Assert.That(result.Any(h => h.CountryCode == "GB"), Is.True);
    }

    #endregion

    #region FetchHolidaysByCountryCodesAndYearAsync Tests

    [Test]
    public async Task FetchHolidaysByCountryCodesAndYearAsync_ReturnsAllHolidayTypes()
    {
        // Arrange
        var holidays = new List<Holiday>
        {
            CreateHoliday("US", new DateTime(2024, 12, 25), "Christmas"),
            CreateHoliday("US", new DateTime(2024, 2, 14), "Valentine's Day", HolidayType.Observance),
            CreateHoliday("US", new DateTime(2024, 10, 31), "Halloween", HolidayType.Observance)
        };

        await _dbContext.Holidays.AddRangeAsync(holidays);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _repository.FetchHolidaysByCountryCodesAndYearAsync(["US"], 2024);

        // Assert
        Assert.That(result, Has.Count.EqualTo(3));
        Assert.That(result.Count(h => h.Type == HolidayType.Public), Is.EqualTo(1));
        Assert.That(result.Count(h => h.Type == HolidayType.Observance), Is.EqualTo(2));
    }

    [Test]
    public async Task FetchHolidaysByCountryCodesAndYearAsync_FiltersCorrectly()
    {
        // Arrange
        var holidays = new List<Holiday>
        {
            CreateHoliday("US", new DateTime(2024, 12, 25), "US 2024"),
            CreateHoliday("US", new DateTime(2023, 12, 25), "US 2023"),
            CreateHoliday("CA", new DateTime(2024, 12, 25), "CA 2024"),
            CreateHoliday("CA", new DateTime(2023, 12, 25), "CA 2023")
        };

        await _dbContext.Holidays.AddRangeAsync(holidays);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _repository.FetchHolidaysByCountryCodesAndYearAsync(
            ["US", "CA"], 2024);

        // Assert
        Assert.That(result, Has.Count.EqualTo(2));
        Assert.That(result.All(h => h.Date.Year == 2024), Is.True);
        Assert.That(result.Any(h => h.Name == "US 2024"), Is.True);
        Assert.That(result.Any(h => h.Name == "CA 2024"), Is.True);
    }

    #endregion

    #region HolidayExistsAsync Tests

    [Test]
    public async Task HolidayExistsAsync_ReturnsTrueForExactMatch()
    {
        // Arrange
        var holiday = CreateHoliday("US", new DateTime(2024, 12, 25), "Christmas Day");
        await _dbContext.Holidays.AddAsync(holiday);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _repository.HolidayExistsAsync("US", new DateTime(2024, 12, 25), "Christmas Day");

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public async Task HolidayExistsAsync_ReturnsFalseForDifferentCountry()
    {
        // Arrange
        var holiday = CreateHoliday("US", new DateTime(2024, 12, 25), "Christmas Day");
        await _dbContext.Holidays.AddAsync(holiday);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _repository.HolidayExistsAsync("CA", new DateTime(2024, 12, 25), "Christmas Day");

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public async Task HolidayExistsAsync_ReturnsFalseForDifferentDate()
    {
        // Arrange
        var holiday = CreateHoliday("US", new DateTime(2024, 12, 25), "Christmas Day");
        await _dbContext.Holidays.AddAsync(holiday);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _repository.HolidayExistsAsync("US", new DateTime(2024, 12, 26), "Christmas Day");

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public async Task HolidayExistsAsync_ReturnsFalseForDifferentName()
    {
        // Arrange
        var holiday = CreateHoliday("US", new DateTime(2024, 12, 25), "Christmas Day");
        await _dbContext.Holidays.AddAsync(holiday);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _repository.HolidayExistsAsync("US", new DateTime(2024, 12, 25), "Xmas");

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public async Task HolidayExistsAsync_IgnoresTimeComponent()
    {
        // Arrange
        var holiday = CreateHoliday("US", new DateTime(2024, 12, 25, 10, 30, 0), "Christmas Day");
        await _dbContext.Holidays.AddAsync(holiday);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _repository.HolidayExistsAsync("US", new DateTime(2024, 12, 25, 15, 45, 0), "Christmas Day");

        // Assert
        Assert.That(result, Is.True, "Should ignore time component and match on date only");
    }

    [Test]
    public async Task HolidayExistsAsync_IsCaseSensitiveForName()
    {
        // Arrange
        var holiday = CreateHoliday("US", new DateTime(2024, 12, 25), "Christmas Day");
        await _dbContext.Holidays.AddAsync(holiday);
        await _dbContext.SaveChangesAsync();

        // Act
        var resultExact = await _repository.HolidayExistsAsync("US", new DateTime(2024, 12, 25), "Christmas Day");
        var resultLower = await _repository.HolidayExistsAsync("US", new DateTime(2024, 12, 25), "christmas day");

        // Assert
        Assert.That(resultExact, Is.True);
        Assert.That(resultLower, Is.False, "Name comparison should be case-sensitive");
    }

    #endregion

    #region Performance and Scale Tests

    [Test]
    public async Task Repository_HandlesLargeNumberOfHolidays()
    {
        // Arrange
        var holidays = Enumerable.Range(1, 365)
            .Select(i => CreateHoliday("US", new DateTime(2024, 1, 1).AddDays(i - 1), $"Holiday {i}"))
            .ToList();

        await _dbContext.Holidays.AddRangeAsync(holidays);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _repository.FetchHolidaysByCountryCodesAndYearAsync(["US"], 2024);
        var exists = await _repository.HolidayExistsAsync("US", new DateTime(2024, 6, 15), "Holiday 167");

        // Assert
        Assert.That(result.Count, Is.EqualTo(365));
        Assert.That(exists, Is.True);
    }

    [Test]
    public async Task HolidayExistsAsync_PerformanceTest_ChecksQuickly()
    {
        // Arrange
        var holidays = Enumerable.Range(1, 1000)
            .Select(i => CreateHoliday("US", new DateTime(2024, 1, 1).AddHours(i), $"Holiday {i}"))
            .ToList();

        await _dbContext.Holidays.AddRangeAsync(holidays);
        await _dbContext.SaveChangesAsync();

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var exists = await _repository.HolidayExistsAsync("US", new DateTime(2024, 1, 1).AddHours(500), "Holiday 500");
        stopwatch.Stop();

        // Assert
        Assert.That(exists, Is.True);
        Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(100),
            "Existence check should be fast with proper indexing");
    }

    #endregion

    #region Helper Methods

    private static Holiday CreateHoliday(string countryCode, DateTime date, string name, HolidayType type = HolidayType.Public)
    {
        return new Holiday
        {
            CountryCode = countryCode,
            Date = date,
            Name = name,
            LocalName = name,
            Type = type,
            Fixed = true,
            Global = true,
            Counties = []
        };
    }

    #endregion
}
