using Accenture_Assessment.Data.Contexts;
using Accenture_Assessment.Data.Models;
using Accenture_Assessment.Data.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Accenture_Assessment.Tests;

/// <summary>
/// Integration tests for CountryRepository using SQLite in-memory database
/// to validate database constraints and behavior that InMemory provider doesn't support.
/// </summary>
[TestFixture]
public class CountryRepositoryConstraintTests
{
    private SqliteConnection _connection = null!;
    private HolidayDbContext _dbContext = null!;
    private CountryRepository _repository = null!;

    [SetUp]
    public async Task Setup()
    {
        // SQLite in-memory database requires keeping connection open
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<HolidayDbContext>()
            .UseSqlite(_connection)
            .Options;

        _dbContext = new HolidayDbContext(options);
        
        // Create the schema
        await _dbContext.Database.EnsureCreatedAsync();
        
        _repository = new CountryRepository(_dbContext);
    }

    [TearDown]
    public void TearDown()
    {
        _dbContext.Dispose();
        _connection.Close();
        _connection.Dispose();
    }

    [Test]
    public async Task AddCountryAsync_ThrowsException_WhenDuplicateCodeAdded()
    {
        // Arrange
        var country1 = new Country { Code = "US", Name = "United States" };
        var country2 = new Country { Code = "US", Name = "United States of America" };

        // Act
        await _repository.AddCountryAsync(country1);

        // Assert - Adding duplicate code should throw exception due to unique index
        Assert.ThrowsAsync<DbUpdateException>(async () => await _repository.AddCountryAsync(country2));
    }

    [Test]
    public async Task AddCountryAsync_AllowsDifferentCodes()
    {
        // Arrange
        var country1 = new Country { Code = "US", Name = "United States" };
        var country2 = new Country { Code = "CA", Name = "Canada" };

        // Act & Assert
        Assert.DoesNotThrowAsync(async () => await _repository.AddCountryAsync(country1));
        Assert.DoesNotThrowAsync(async () => await _repository.AddCountryAsync(country2));
        
        var countries = await _repository.FetchCountriesAsync();
        Assert.That(countries.Count, Is.EqualTo(2));
    }

    [Test]
    public async Task CountryCode_UniqueIndex_IsCaseSensitiveInSQLite()
    {
        // Arrange
        var country1 = new Country { Code = "US", Name = "United States" };
        var country2 = new Country { Code = "us", Name = "United States (lowercase)" };

        // Act & Assert
        await _repository.AddCountryAsync(country1);
        
        // SQLite is case-sensitive by default, so this should succeed
        Assert.DoesNotThrowAsync(async () => await _repository.AddCountryAsync(country2));
        
        var countries = await _repository.FetchCountriesAsync();
        Assert.That(countries.Count, Is.EqualTo(2));
    }
}
