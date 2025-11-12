# Understanding Test Database Providers

## The Problem

Your test `AddCountryAsync_ThrowsException_WhenDuplicateCodeAdded` was failing because:

```csharp
[Test]
public void AddCountryAsync_ThrowsException_WhenDuplicateCodeAdded()
{
    var country1 = new Country { Code = "US", Name = "United States" };
    var country2 = new Country { Code = "US", Name = "United States of America" };
    
    await _repository.AddCountryAsync(country1); // ? Succeeds
    await _repository.AddCountryAsync(country2); // ? Should throw, but doesn't!
}
```

**Expected:** `DbUpdateException` (unique constraint violation)  
**Actual:** No exception - both countries saved successfully

## Root Cause

**EF Core InMemory database does NOT enforce database constraints:**
- ? No unique indexes
- ? No foreign keys
- ? No check constraints
- ? No cascading deletes

This is by design! InMemory is meant for **unit testing business logic**, not database behavior.

## The Solution

We now have **two test classes** with different purposes:

### 1. CountryRepositoryTests (EF InMemory) - 18 Tests

**Purpose:** Unit test business logic  
**Database:** EF Core InMemory  
**Speed:** ? Very Fast (~500ms)

```csharp
[SetUp]
public void Setup()
{
    var options = new DbContextOptionsBuilder<HolidayDbContext>()
        .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
        .Options;
    
    _dbContext = new HolidayDbContext(options);
    _repository = new CountryRepository(_dbContext);
}
```

**Tests:**
- ? Fetch operations
- ? Add operations
- ? Exists checks
- ? Sorting
- ? Performance
- ?? **Skips:** Constraint validation (marked with `[Ignore]`)

### 2. CountryRepositoryConstraintTests (SQLite) - 3 Tests

**Purpose:** Validate database constraints  
**Database:** SQLite in-memory  
**Speed:** ? Fast (~1000ms)

```csharp
[SetUp]
public async Task Setup()
{
    // SQLite in-memory requires keeping connection open
    _connection = new SqliteConnection("DataSource=:memory:");
    _connection.Open();
    
    var options = new DbContextOptionsBuilder<HolidayDbContext>()
        .UseSqlite(_connection)
        .Options;
    
    _dbContext = new HolidayDbContext(options);
    await _dbContext.Database.EnsureCreatedAsync(); // Creates schema with constraints
    
    _repository = new CountryRepository(_dbContext);
}
```

**Tests:**
- ? Unique constraint enforcement
- ? Duplicate code rejection
- ? Case sensitivity behavior

---

## When to Use Each Provider

### Use EF InMemory When:
- ? Testing business logic
- ? Testing CRUD operations
- ? Testing queries and sorting
- ? Need very fast tests
- ? Testing service layer

### Use SQLite InMemory When:
- ? Testing database constraints
- ? Testing unique indexes
- ? Testing foreign key behavior
- ? Testing transactions
- ? Need realistic database behavior

### Use Real SQL Server When:
- ? Integration testing
- ? Testing migrations
- ? Testing SQL Server-specific features
- ? Testing collation behavior
- ? End-to-end testing

---

## Test Results

### Before Fix:
```
Failed!  - Failed:     1, Passed:    18, Skipped:     0, Total:    19
AddCountryAsync_ThrowsException_WhenDuplicateCodeAdded: FAILED
  Expected: DbUpdateException
  But was:  null
```

### After Fix:
```
Passed!  - Failed:     0, Passed:    18, Skipped:     1, Total:    19
AddCountryAsync_ThrowsException_WhenDuplicateCodeAdded: SKIPPED
  Reason: InMemory database does not enforce unique constraints
```

**Plus 3 new constraint tests with SQLite:**
```
Passed!  - Failed:     0, Passed:     3, Skipped:     0, Total:     3
All constraint tests passing with SQLite
```

---

## Code Changes Made

### 1. Marked Original Test as Ignored
```csharp
[Test]
[Ignore("InMemory database does not enforce unique constraints. " +
        "Use integration tests with real database for constraint validation.")]
public void AddCountryAsync_ThrowsException_WhenDuplicateCodeAdded()
{
    // Original test code...
}
```

### 2. Created New Constraint Test Class
```csharp
[TestFixture]
public class CountryRepositoryConstraintTests
{
    private SqliteConnection _connection = null!;
    private HolidayDbContext _dbContext = null!;
    private CountryRepository _repository = null!;

    [SetUp]
    public async Task Setup()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open(); // Must stay open for in-memory to persist
        
        var options = new DbContextOptionsBuilder<HolidayDbContext>()
            .UseSqlite(_connection)
            .Options;
        
        _dbContext = new HolidayDbContext(options);
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

        // Assert - This NOW throws as expected!
        Assert.ThrowsAsync<DbUpdateException>(
            async () => await _repository.AddCountryAsync(country2));
    }
}
```

### 3. Added NuGet Packages
```xml
<PackageReference Include="Microsoft.Data.Sqlite" Version="9.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="9.0.0" />
```

---

## Key Learnings

### 1. InMemory is NOT a Real Database
It's a **fake** that mimics some database behavior but skips constraints for speed.

### 2. SQLite is Close to Real
SQLite enforces most constraints and is still fast for testing.

### 3. Test the Right Thing
- **Business logic** ? InMemory (fast)
- **Database behavior** ? SQLite (realistic)
- **Full integration** ? SQL Server (production-like)

### 4. Document Test Limitations
Always document when tests are skipped and why.

---

## Running the Tests

### Run Unit Tests Only (Fast)
```bash
dotnet test --filter "FullyQualifiedName~CountryRepositoryTests&FullyQualifiedName!~Constraint"
```

### Run Constraint Tests Only
```bash
dotnet test --filter "FullyQualifiedName~CountryRepositoryConstraintTests"
```

### Run All Repository Tests
```bash
dotnet test --filter "FullyQualifiedName~CountryRepository"
```

---

## Summary

? **Problem Solved:** Constraint test now uses SQLite and passes  
? **Fast Tests Remain:** 18 unit tests still use InMemory for speed  
? **Best of Both Worlds:** Fast unit tests + realistic constraint tests  
? **Documented:** Clear explanation of which tests use which provider  

**Total Repository Tests:** 21 (18 unit + 3 constraint)  
**Execution Time:** ~1.5 seconds for all  
**All Passing:** ?
