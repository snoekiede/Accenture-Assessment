using Accenture_Assessment.Data.Contexts;
using Accenture_Assessment.Data.Models;
using Accenture_Assessment.Data.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Accenture_Assessment.Tests;

[TestFixture]
public class CountryRepositoryTests
{
    private HolidayDbContext _dbContext = null!;
    private CountryRepository _repository = null!;

    [SetUp]
    public void Setup()
    {
        // Create a new in-memory database for each test
        var options = new DbContextOptionsBuilder<HolidayDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new HolidayDbContext(options);
        _repository = new CountryRepository(_dbContext);
    }

    [TearDown]
    public void TearDown()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }

    [Test]
    public async Task FetchCountriesAsync_ReturnsEmptyList_WhenNoCountriesExist()
    {
        // Act
        var result = await _repository.FetchCountriesAsync();

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.Empty);
    }

    [Test]
    public async Task FetchCountriesAsync_ReturnsAllCountries_WhenCountriesExist()
    {
        // Arrange
        var countries = new List<Country>
        {
            new Country { Code = "US", Name = "United States" },
            new Country { Code = "CA", Name = "Canada" },
            new Country { Code = "GB", Name = "United Kingdom" }
        };

        await _dbContext.Countries.AddRangeAsync(countries);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _repository.FetchCountriesAsync();

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(3));
    }

    [Test]
    public async Task FetchCountriesAsync_ReturnsCountriesSortedByName()
    {
        // Arrange
        var countries = new List<Country>
        {
            new Country { Code = "US", Name = "United States" },
            new Country { Code = "AT", Name = "Austria" },
            new Country { Code = "GB", Name = "United Kingdom" },
            new Country { Code = "CA", Name = "Canada" }
        };

        await _dbContext.Countries.AddRangeAsync(countries);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _repository.FetchCountriesAsync();

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(4));
        Assert.That(result[0].Name, Is.EqualTo("Austria"));
        Assert.That(result[1].Name, Is.EqualTo("Canada"));
        Assert.That(result[2].Name, Is.EqualTo("United Kingdom"));
        Assert.That(result[3].Name, Is.EqualTo("United States"));
    }

    [Test]
    public async Task AddCountryAsync_AddsCountrySuccessfully()
    {
        // Arrange
        var country = new Country { Code = "US", Name = "United States" };

        // Act
        var result = await _repository.AddCountryAsync(country);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.GreaterThan(0));
        Assert.That(result.Code, Is.EqualTo("US"));
        Assert.That(result.Name, Is.EqualTo("United States"));

        // Verify it's in the database
        var savedCountry = await _dbContext.Countries.FirstOrDefaultAsync(c => c.Code == "US");
        Assert.That(savedCountry, Is.Not.Null);
        Assert.That(savedCountry!.Name, Is.EqualTo("United States"));
    }

    [Test]
    public async Task AddCountryAsync_AssignsId()
    {
        // Arrange
        var country = new Country { Code = "US", Name = "United States" };

        // Act
        var result = await _repository.AddCountryAsync(country);

        // Assert
        Assert.That(result.Id, Is.GreaterThan(0), "Id should be assigned after saving");
    }

    [Test]
    public async Task AddCountryAsync_MultipleCountries_AssignsUniqueIds()
    {
        // Arrange
        var country1 = new Country { Code = "US", Name = "United States" };
        var country2 = new Country { Code = "CA", Name = "Canada" };

        // Act
        var result1 = await _repository.AddCountryAsync(country1);
        var result2 = await _repository.AddCountryAsync(country2);

        // Assert
        Assert.That(result1.Id, Is.Not.EqualTo(result2.Id), "Each country should have a unique Id");
        Assert.That(result1.Id, Is.GreaterThan(0));
        Assert.That(result2.Id, Is.GreaterThan(0));
    }


    [Test]
    public async Task CountryExistsAsync_ReturnsFalse_WhenCountryDoesNotExist()
    {
        // Act
        var result = await _repository.CountryExistsAsync("US");

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public async Task CountryExistsAsync_ReturnsTrue_WhenCountryExists()
    {
        // Arrange
        var country = new Country { Code = "US", Name = "United States" };
        await _dbContext.Countries.AddAsync(country);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _repository.CountryExistsAsync("US");

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public async Task CountryExistsAsync_IsCaseSensitive()
    {
        // Arrange
        var country = new Country { Code = "US", Name = "United States" };
        await _dbContext.Countries.AddAsync(country);
        await _dbContext.SaveChangesAsync();

        // Act
        var resultUpperCase = await _repository.CountryExistsAsync("US");
        var resultLowerCase = await _repository.CountryExistsAsync("us");

        // Assert
        Assert.That(resultUpperCase, Is.True);
        Assert.That(resultLowerCase, Is.False, "Country code comparison should be case-sensitive");
    }

    [Test]
    public async Task CountryExistsAsync_ReturnsFalse_ForEmptyString()
    {
        // Arrange
        var country = new Country { Code = "US", Name = "United States" };
        await _dbContext.Countries.AddAsync(country);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _repository.CountryExistsAsync("");

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public async Task CountryExistsAsync_ChecksMultipleCountries()
    {
        // Arrange
        var countries = new List<Country>
        {
            new Country { Code = "US", Name = "United States" },
            new Country { Code = "CA", Name = "Canada" },
            new Country { Code = "GB", Name = "United Kingdom" }
        };

        await _dbContext.Countries.AddRangeAsync(countries);
        await _dbContext.SaveChangesAsync();

        // Act
        var usExists = await _repository.CountryExistsAsync("US");
        var caExists = await _repository.CountryExistsAsync("CA");
        var gbExists = await _repository.CountryExistsAsync("GB");
        var frExists = await _repository.CountryExistsAsync("FR");

        // Assert
        Assert.That(usExists, Is.True);
        Assert.That(caExists, Is.True);
        Assert.That(gbExists, Is.True);
        Assert.That(frExists, Is.False);
    }

    [Test]
    public async Task AddCountryAsync_AndFetchCountriesAsync_WorkTogether()
    {
        // Arrange & Act
        await _repository.AddCountryAsync(new Country { Code = "US", Name = "United States" });
        await _repository.AddCountryAsync(new Country { Code = "CA", Name = "Canada" });
        
        var countries = await _repository.FetchCountriesAsync();

        // Assert
        Assert.That(countries.Count, Is.EqualTo(2));
        Assert.That(countries.Any(c => c.Code == "US"), Is.True);
        Assert.That(countries.Any(c => c.Code == "CA"), Is.True);
    }

    [Test]
    public async Task FetchCountriesAsync_ReturnsCountriesWithAllProperties()
    {
        // Arrange
        var country = new Country 
        { 
            Code = "US", 
            Name = "United States" 
        };

        await _dbContext.Countries.AddAsync(country);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _repository.FetchCountriesAsync();

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(result[0].Id, Is.GreaterThan(0));
        Assert.That(result[0].Code, Is.EqualTo("US"));
        Assert.That(result[0].Name, Is.EqualTo("United States"));
    }

    [Test]
    public async Task Repository_HandlesLargeNumberOfCountries()
    {
        // Arrange
        var countries = Enumerable.Range(1, 100)
            .Select(i => new Country 
            { 
                Code = $"C{i:D2}", 
                Name = $"Country {i}" 
            })
            .ToList();

        await _dbContext.Countries.AddRangeAsync(countries);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _repository.FetchCountriesAsync();
        var exists = await _repository.CountryExistsAsync("C50");

        // Assert
        Assert.That(result.Count, Is.EqualTo(100));
        Assert.That(exists, Is.True);
    }

    [Test]
    public async Task AddCountryAsync_ReturnsCountryWithSameReferenceValues()
    {
        // Arrange
        var country = new Country { Code = "US", Name = "United States" };

        // Act
        var result = await _repository.AddCountryAsync(country);

        // Assert
        Assert.That(result, Is.SameAs(country), "Should return the same country instance");
        Assert.That(result.Code, Is.EqualTo(country.Code));
        Assert.That(result.Name, Is.EqualTo(country.Name));
    }

    [Test]
    public async Task CountryExistsAsync_PerformanceTest_ChecksQuickly()
    {
        // Arrange
        var countries = Enumerable.Range(1, 1000)
            .Select(i => new Country 
            { 
                Code = $"C{i:D3}", 
                Name = $"Country {i}" 
            })
            .ToList();

        await _dbContext.Countries.AddRangeAsync(countries);
        await _dbContext.SaveChangesAsync();

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var exists = await _repository.CountryExistsAsync("C500");
        stopwatch.Stop();

        // Assert
        Assert.That(exists, Is.True);
        Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(100), 
            "Existence check should be fast with proper indexing");
    }
}
