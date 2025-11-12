# Contributing to Accenture Assessment - Holiday Information System

Thank you for your interest in contributing to this project! While this is primarily an assessment/demonstration project, contributions that improve code quality, add features, or enhance documentation are welcome.

## ?? Table of Contents

- [Code of Conduct](#code-of-conduct)
- [Getting Started](#getting-started)
- [Development Setup](#development-setup)
- [Project Structure](#project-structure)
- [Development Workflow](#development-workflow)
- [Coding Standards](#coding-standards)
- [Testing Guidelines](#testing-guidelines)
- [Commit Guidelines](#commit-guidelines)
- [Pull Request Process](#pull-request-process)
- [Documentation](#documentation)
- [Questions and Support](#questions-and-support)

---

## ?? Code of Conduct

### Our Pledge

We are committed to providing a welcoming and inspiring community for all. Please be respectful and constructive in all interactions.

### Our Standards

**Positive behaviors include:**
- ? Using welcoming and inclusive language
- ? Being respectful of differing viewpoints
- ? Gracefully accepting constructive criticism
- ? Focusing on what is best for the community
- ? Showing empathy towards others

**Unacceptable behaviors include:**
- ? Trolling, insulting/derogatory comments
- ? Public or private harassment
- ? Publishing others' private information
- ? Other conduct which could reasonably be considered inappropriate

---

## ?? Getting Started

### Prerequisites

Before you begin, ensure you have:

- ? **.NET 9 SDK** - [Download](https://dotnet.microsoft.com/download/dotnet/9.0)
- ? **Docker Desktop** - [Download](https://www.docker.com/products/docker-desktop)
- ? **.NET Aspire workload** - `dotnet workload install aspire`
- ? **Git** - [Download](https://git-scm.com/downloads)
- ? **IDE**: Visual Studio 2022 (17.13+), Rider 2024.3+, or VS Code with C# Dev Kit

### Fork and Clone

1. **Fork the repository** on GitHub
2. **Clone your fork**:
   ```bash
   git clone https://github.com/YOUR_USERNAME/Accenture-Assessment.git
   cd Accenture-Assessment
   ```
3. **Add upstream remote**:
   ```bash
   git remote add upstream https://github.com/snoekiede/Accenture-Assessment.git
   ```
4. **Verify remotes**:
   ```bash
   git remote -v
   # origin    https://github.com/YOUR_USERNAME/Accenture-Assessment.git (fetch)
   # origin    https://github.com/YOUR_USERNAME/Accenture-Assessment.git (push)
   # upstream  https://github.com/snoekiede/Accenture-Assessment.git (fetch)
   # upstream  https://github.com/snoekiede/Accenture-Assessment.git (push)
   ```

---

## ??? Development Setup

### 1. Install Dependencies

```bash
# Restore NuGet packages
dotnet restore

# Install Aspire workload (if not already installed)
dotnet workload install aspire
```

### 2. Build the Solution

```bash
# Build all projects
dotnet build

# Or build in Release mode
dotnet build -c Release
```

### 3. Apply Database Migrations

```bash
dotnet ef database update --project Accenture-Assessment.Data --startup-project Accenture-Assessment.ApiService
```

### 4. Run the Application

```bash
# Using Aspire (recommended)
dotnet run --project Accenture-Assessment.AppHost

# Access the application
# Web UI: https://localhost:7148
# API: https://localhost:7001
# Aspire Dashboard: http://localhost:15000
```

### 5. Run Tests

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

---

## ?? Project Structure

Understanding the project structure is crucial for contributions:

```
Accenture-Assessment/
??? Accenture-Assessment.Web/              # Blazor Server UI
?   ??? Components/Pages/                  # Razor pages
?   ??? Program.cs                         # Web host configuration
?
??? Accenture-Assessment.ApiService/       # ASP.NET Core API
?   ??? Program.cs                         # API endpoints & middleware
?   ??? appsettings.json                   # Configuration
?
??? Accenture-Assessment.Data/             # Data Access Layer
?   ??? Contexts/                          # EF Core DbContext
?   ??? Models/                            # Entity models
?   ??? Repositories/                      # Repository pattern
?   ??? Services/                          # Business logic
?   ??? Interfaces/                        # Abstractions
?   ??? Migrations/                        # EF Core migrations
?
??? Accenture-Assessment.Contracts/        # Shared DTOs
?   ??? Dtos/                              # Data Transfer Objects
?   ??? Enums/                             # Enumerations
?
??? Accenture-Assessment.AppHost/          # Aspire Orchestration
??? Accenture-Assessment.ServiceDefaults/  # Aspire Service Defaults
?
??? Accenture-Assessment.Tests/            # Test Project
    ??? WebTests.cs                        # Integration tests
    ??? *RepositoryTests.cs                # Unit tests
    ??? *ServiceTests.cs                   # Service tests
```

### Key Principles

- **Clean Architecture** - Separation of concerns
- **Repository Pattern** - Data access abstraction
- **Dependency Injection** - Loose coupling
- **DTOs** - No domain models in API responses
- **Interface Segregation** - Small, focused interfaces

---

## ?? Development Workflow

### 1. Create a Feature Branch

```bash
# Update your main branch
git checkout main
git pull upstream main

# Create a feature branch
git checkout -b feature/your-feature-name

# Or for bug fixes
git checkout -b fix/issue-description
```

### Branch Naming Conventions

- `feature/` - New features (e.g., `feature/add-holiday-categories`)
- `fix/` - Bug fixes (e.g., `fix/date-timezone-issue`)
- `docs/` - Documentation updates (e.g., `docs/improve-readme`)
- `refactor/` - Code refactoring (e.g., `refactor/simplify-service`)
- `test/` - Test additions/improvements (e.g., `test/add-api-tests`)
- `chore/` - Maintenance tasks (e.g., `chore/update-dependencies`)

### 2. Make Your Changes

Follow the [Coding Standards](#coding-standards) and [Testing Guidelines](#testing-guidelines).

### 3. Test Your Changes

```bash
# Run all tests
dotnet test

# Run specific test category
dotnet test --filter "FullyQualifiedName~YourTests"

# Verify build
dotnet build

# Run the application
dotnet run --project Accenture-Assessment.AppHost
```

### 4. Commit Your Changes

Follow the [Commit Guidelines](#commit-guidelines):

```bash
git add .
git commit -m "feat: add holiday category filtering"
```

### 5. Push to Your Fork

```bash
git push origin feature/your-feature-name
```

### 6. Open a Pull Request

Go to GitHub and create a Pull Request from your fork to the upstream repository.

---

## ?? Coding Standards

### C# Style Guide

We follow **Microsoft's C# Coding Conventions** with some project-specific rules:

#### Naming Conventions

```csharp
// Classes, Methods, Properties - PascalCase
public class HolidayService { }
public void GetHolidays() { }
public string CountryName { get; set; }

// Private fields - camelCase with underscore prefix
private readonly IHolidayRepository _holidayRepository;

// Parameters, local variables - camelCase
public void ProcessHoliday(string countryCode, DateTime date) { }

// Constants - PascalCase
public const int MaxRetryAttempts = 3;

// Interfaces - PascalCase with 'I' prefix
public interface IHolidayDataService { }
```

#### Code Structure

```csharp
// ? GOOD - Clear, single responsibility
public class HolidayService
{
    private readonly IHolidayRepository _repository;
    private readonly ILogger<HolidayService> _logger;

    public HolidayService(IHolidayRepository repository, ILogger<HolidayService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<List<Holiday>> GetHolidaysAsync(string countryCode)
    {
        _logger.LogInformation("Fetching holidays for {CountryCode}", countryCode);
        return await _repository.FetchHolidaysByCountryCodeAsync(countryCode);
    }
}

// ? BAD - Too much responsibility, no logging
public class HolidayService
{
    public async Task<List<Holiday>> GetHolidays(string code)
    {
        var holidays = await GetFromDb(code);
        if (!holidays.Any())
        {
            holidays = await CallApi(code);
            SaveToDb(holidays);
        }
        return holidays.Where(h => h.Date > DateTime.Now).OrderBy(h => h.Date).ToList();
    }
}
```

#### File Organization

```csharp
// 1. Using statements (sorted)
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using YourProject.Models;

// 2. Namespace
namespace YourProject.Services
{
    // 3. Class with XML documentation
    /// <summary>
    /// Service for managing holiday data operations.
    /// </summary>
    public class HolidayService : IHolidayService
    {
        // 4. Fields
        private readonly IHolidayRepository _repository;
        
        // 5. Constructor
        public HolidayService(IHolidayRepository repository)
        {
            _repository = repository;
        }
        
        // 6. Public methods
        public async Task<List<Holiday>> GetHolidaysAsync()
        {
            // Implementation
        }
        
        // 7. Private methods
        private bool IsValidDate(DateTime date)
        {
            // Implementation
        }
    }
}
```

### .NET 9 Features

Use modern C# features where appropriate:

```csharp
// ? Primary constructors
public class HolidayService(IHolidayRepository repository, ILogger<HolidayService> logger)
{
    // Fields automatically created from parameters
}

// ? Collection expressions
var holidays = [holiday1, holiday2, holiday3];

// ? Required properties
public required string CountryCode { get; set; }

// ? Target-typed new
Holiday holiday = new() { Name = "Christmas", Date = date };

// ? Null-coalescing assignment
Counties ??= new List<string>();
```

### ASP.NET Core Minimal APIs

```csharp
// ? GOOD - Clear endpoint with validation and documentation
app.MapGet("/api/holidays/{countryCode}", async (
    string countryCode,
    IHolidayDataService service,
    ILogger<Program> logger) =>
{
    if (!ValidateCountryCode(countryCode))
        return Results.BadRequest("Invalid country code format.");

    logger.LogInformation("Fetching holidays for {CountryCode}", countryCode);
    var holidays = await service.GetHolidaysAsync(countryCode);
    return Results.Ok(holidays);
})
.WithName("GetHolidays")
.WithOpenApi()
.WithDescription("Retrieves all holidays for a specific country");

// ? BAD - No validation, no logging, no documentation
app.MapGet("/api/holidays/{countryCode}", async (string countryCode, IHolidayDataService service) =>
    await service.GetHolidaysAsync(countryCode));
```

### Blazor Components

```razor
@* ? GOOD - Clear structure, proper binding *@
@page "/holidays"
@inject IHolidayService HolidayService
@inject ILogger<Holidays> Logger

<h3>Holidays</h3>

@if (_isLoading)
{
    <p><em>Loading...</em></p>
}
else if (_errorMessage != null)
{
    <div class="alert alert-danger">@_errorMessage</div>
}
else
{
    <ul>
        @foreach (var holiday in _holidays)
        {
            <li>@holiday.Name - @holiday.Date.ToShortDateString()</li>
        }
    </ul>
}

@code {
    private List<Holiday> _holidays = new();
    private bool _isLoading = true;
    private string? _errorMessage;

    protected override async Task OnInitializedAsync()
    {
        try
        {
            _holidays = await HolidayService.GetHolidaysAsync();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to load holidays");
            _errorMessage = "Failed to load holidays. Please try again.";
        }
        finally
        {
            _isLoading = false;
        }
    }
}
```

---

## ?? Testing Guidelines

### Test Structure

We use **NUnit** with the **AAA pattern** (Arrange, Act, Assert):

```csharp
[Test]
public async Task MethodName_ExpectedBehavior_Condition()
{
    // Arrange - Set up test data and mocks
    var mockRepo = new Mock<IHolidayRepository>();
    mockRepo.Setup(x => x.GetHolidaysAsync("US"))
        .ReturnsAsync(new List<Holiday> { testHoliday });
    var service = new HolidayService(mockRepo.Object);

    // Act - Execute the method under test
    var result = await service.GetHolidaysAsync("US");

    // Assert - Verify the results
    Assert.That(result, Is.Not.Null);
    Assert.That(result, Has.Count.EqualTo(1));
    mockRepo.Verify(x => x.GetHolidaysAsync("US"), Times.Once);
}
```

### Test Categories

#### 1. Unit Tests

Test individual methods in isolation using mocks:

```csharp
[TestFixture]
public class HolidayServiceTests
{
    private Mock<IHolidayRepository> _mockRepository;
    private HolidayService _service;

    [SetUp]
    public void Setup()
    {
        _mockRepository = new Mock<IHolidayRepository>();
        _service = new HolidayService(_mockRepository.Object);
    }

    [Test]
    public async Task GetHolidays_ReturnsHolidays_WhenCountryExists()
    {
        // Test implementation
    }
}
```

#### 2. Integration Tests

Test the full application stack:

```csharp
[TestFixture]
public class ApiIntegrationTests
{
    [Test]
    public async Task GetHolidays_ReturnsOk_WithValidCountryCode()
    {
        // Arrange
        var appHost = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.Accenture_Assessment_AppHost>(cancellationToken);
        
        await using var app = await appHost.BuildAsync();
        await app.StartAsync();

        // Act
        var httpClient = app.CreateHttpClient("apiservice");
        var response = await httpClient.GetAsync("/api/holidays/US");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }
}
```

#### 3. Repository Tests

Use InMemory database for fast unit tests:

```csharp
[TestFixture]
public class HolidayRepositoryTests
{
    private HolidayDbContext _dbContext;
    private HolidayRepository _repository;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<HolidayDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
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
}
```

### Test Coverage Requirements

- ? **Unit Tests**: All public methods in services and repositories
- ? **Integration Tests**: All API endpoints
- ? **Edge Cases**: Null inputs, empty collections, boundary conditions
- ? **Error Scenarios**: Exception handling, validation failures

### Running Tests

```bash
# All tests
dotnet test

# Specific test class
dotnet test --filter "FullyQualifiedName~HolidayServiceTests"

# Specific test method
dotnet test --filter "FullyQualifiedName~HolidayServiceTests.GetHolidays_ReturnsHolidays"

# With detailed output
dotnet test --logger "console;verbosity=detailed"

# With coverage
dotnet test --collect:"XPlat Code Coverage"
```

---

## ?? Commit Guidelines

We follow **Conventional Commits** specification:

### Commit Message Format

```
<type>(<scope>): <subject>

<body>

<footer>
```

### Types

- `feat` - New feature
- `fix` - Bug fix
- `docs` - Documentation changes
- `style` - Code style changes (formatting, no logic change)
- `refactor` - Code refactoring (no feature change)
- `perf` - Performance improvements
- `test` - Adding or updating tests
- `chore` - Maintenance tasks (dependencies, build, etc.)
- `ci` - CI/CD changes

### Examples

```bash
# Feature
git commit -m "feat(api): add holiday category filtering endpoint"

# Bug fix
git commit -m "fix(web): correct date timezone conversion in UI"

# Documentation
git commit -m "docs(readme): update installation instructions"

# Refactoring
git commit -m "refactor(service): simplify holiday retrieval logic"

# Test
git commit -m "test(repository): add tests for weekend filtering"

# Breaking change
git commit -m "feat(api): change holiday response format

BREAKING CHANGE: Holiday API now returns ISO 8601 dates"
```

### Commit Best Practices

- ? Write clear, concise commit messages
- ? Use present tense ("add feature" not "added feature")
- ? Capitalize the first letter
- ? No period at the end of the subject line
- ? Separate subject from body with blank line
- ? Limit subject line to 72 characters
- ? Reference issues/PRs in the footer

---

## ?? Pull Request Process

### Before Submitting

1. ? **Update your branch**:
   ```bash
   git checkout main
   git pull upstream main
   git checkout your-feature-branch
   git rebase main
   ```

2. ? **Run all tests**:
   ```bash
   dotnet test
   ```

3. ? **Verify build**:
   ```bash
   dotnet build
   ```

4. ? **Run the application**:
   ```bash
   dotnet run --project Accenture-Assessment.AppHost
   ```

5. ? **Update documentation** if needed

### PR Title

Follow the same format as commit messages:

```
feat(api): add holiday category filtering
fix(web): correct timezone handling in calendar
docs: update API documentation
```

### PR Description Template

```markdown
## Description
Brief description of changes

## Type of Change
- [ ] Bug fix (non-breaking change which fixes an issue)
- [ ] New feature (non-breaking change which adds functionality)
- [ ] Breaking change (fix or feature that would cause existing functionality to not work as expected)
- [ ] Documentation update

## How Has This Been Tested?
Describe the tests you ran

## Checklist
- [ ] My code follows the style guidelines of this project
- [ ] I have performed a self-review of my own code
- [ ] I have commented my code, particularly in hard-to-understand areas
- [ ] I have made corresponding changes to the documentation
- [ ] My changes generate no new warnings
- [ ] I have added tests that prove my fix is effective or that my feature works
- [ ] New and existing unit tests pass locally with my changes
- [ ] Any dependent changes have been merged and published

## Related Issues
Fixes #123
Closes #456
```

### Review Process

1. **Automated Checks**: CI/CD pipeline runs tests and build
2. **Code Review**: Maintainer reviews code for quality and standards
3. **Feedback**: Address any review comments
4. **Approval**: Once approved, PR will be merged

### After Merge

```bash
# Update your local main branch
git checkout main
git pull upstream main

# Delete your feature branch
git branch -d feature/your-feature-name
git push origin --delete feature/your-feature-name
```

---

## ?? Documentation

### Code Documentation

Use XML documentation comments for public APIs:

```csharp
/// <summary>
/// Retrieves all holidays for a specific country and year.
/// </summary>
/// <param name="countryCode">The ISO 3166-1 alpha-2 country code.</param>
/// <param name="year">The year for which to retrieve holidays.</param>
/// <returns>A list of holidays for the specified country and year.</returns>
/// <exception cref="ArgumentException">Thrown when country code is invalid.</exception>
public async Task<List<Holiday>> GetHolidaysAsync(string countryCode, int year)
{
    // Implementation
}
```

### Documentation Files

When updating documentation:

- **README.md** - General project information
- **CONTRIBUTING.md** - This file
- **API.md** - API endpoint documentation (if creating)
- **CHANGELOG.md** - Version history (if creating)
- Code comments for complex logic

---

## ? Questions and Support

### Getting Help

- ?? **Questions**: Open a [GitHub Discussion](https://github.com/snoekiede/Accenture-Assessment/discussions)
- ?? **Bug Reports**: Open a [GitHub Issue](https://github.com/snoekiede/Accenture-Assessment/issues)
- ?? **Feature Requests**: Open a [GitHub Issue](https://github.com/snoekiede/Accenture-Assessment/issues) with "enhancement" label

### Issue Template

```markdown
## Description
A clear description of the issue

## Steps to Reproduce
1. Step one
2. Step two
3. ...

## Expected Behavior
What you expected to happen

## Actual Behavior
What actually happened

## Environment
- OS: [e.g., Windows 11, macOS 14]
- .NET Version: [e.g., 9.0.1]
- Browser (if applicable): [e.g., Chrome 120]

## Additional Context
Any other relevant information
```

---

## ?? Areas for Contribution

We welcome contributions in these areas:

### High Priority

- ? **Tests** - Increase test coverage
- ? **Documentation** - Improve existing docs
- ? **Bug Fixes** - Fix reported issues
- ? **Performance** - Optimize slow operations

### Medium Priority

- ?? **Refactoring** - Improve code quality
- ?? **UI/UX** - Enhance user interface
- ?? **Localization** - Add multi-language support
- ?? **Monitoring** - Add Application Insights

### Low Priority

- ? **Features** - New functionality (discuss first)
- ?? **Security** - Authentication/authorization
- ?? **Mobile** - Responsive improvements

---

## ?? License

By contributing, you agree that your contributions will be licensed under the same license as the project (MIT License).

---

## ?? Thank You!

Thank you for taking the time to contribute! Every contribution, no matter how small, helps make this project better.

### Recognition

Contributors will be recognized in:
- ?? CHANGELOG.md (when created)
- ?? GitHub Contributors page
- ?? Project documentation

---

## ?? Additional Resources

- [.NET 9 Documentation](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-9/overview)
- [Blazor Documentation](https://learn.microsoft.com/en-us/aspnet/core/blazor/)
- [.NET Aspire Documentation](https://learn.microsoft.com/en-us/dotnet/aspire/)
- [Entity Framework Core](https://learn.microsoft.com/en-us/ef/core/)
- [NUnit Documentation](https://docs.nunit.org/)
- [Conventional Commits](https://www.conventionalcommits.org/)

---

**Happy Contributing! ??**

*Last Updated: 2024*
