using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using Accenture_Assessment.Contracts.Dtos;

namespace Accenture_Assessment.Tests;

public class WebTests
{
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(30);

    [Test]
    public async Task GetWebResourceRootReturnsOkStatusCode()
    {
        // Arrange
        var cancellationToken = TestContext.CurrentContext.CancellationToken;

        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.Accenture_Assessment_AppHost>(cancellationToken);
        appHost.Services.AddLogging(logging =>
        {
            logging.SetMinimumLevel(LogLevel.Debug);
            // Override the logging filters from the app's configuration
            logging.AddFilter(appHost.Environment.ApplicationName, LogLevel.Debug);
            logging.AddFilter("Aspire.", LogLevel.Debug);
        });
        appHost.Services.ConfigureHttpClientDefaults(clientBuilder =>
        {
            clientBuilder.AddStandardResilienceHandler();
        });

        await using var app = await appHost.BuildAsync(cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);
        await app.StartAsync(cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);

        // Act
        var httpClient = app.CreateHttpClient("webfrontend");
        await app.ResourceNotifications.WaitForResourceHealthyAsync("webfrontend", cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);
        var response = await httpClient.GetAsync("/", cancellationToken);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task GetCountriesEndpointReturnsOkStatusCode()
    {
        // Arrange
        var cancellationToken = TestContext.CurrentContext.CancellationToken;
        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.Accenture_Assessment_AppHost>(cancellationToken);
        
        await using var app = await appHost.BuildAsync(cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);
        await app.StartAsync(cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);

        // Act
        var httpClient = app.CreateHttpClient("apiservice");
        await app.ResourceNotifications.WaitForResourceHealthyAsync("apiservice", cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);
        var response = await httpClient.GetAsync("/api/countries", cancellationToken);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        Assert.That(content, Is.Not.Null);
        Assert.That(content, Does.Contain("countryCode"));
    }

    [Test]
    public async Task GetCountriesEndpointReturnsValidJson()
    {
        // Arrange
        var cancellationToken = TestContext.CurrentContext.CancellationToken;
        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.Accenture_Assessment_AppHost>(cancellationToken);
        
        await using var app = await appHost.BuildAsync(cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);
        await app.StartAsync(cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);

        // Act
        var httpClient = app.CreateHttpClient("apiservice");
        await app.ResourceNotifications.WaitForResourceHealthyAsync("apiservice", cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);
        var countries = await httpClient.GetFromJsonAsync<List<CountryDto>>("/api/countries", cancellationToken);

        // Assert
        Assert.That(countries, Is.Not.Null);
        Assert.That(countries, Is.Not.Empty);
        Assert.That(countries![0].countryCode, Is.Not.Null);
        Assert.That(countries[0].name, Is.Not.Null);
    }

    [Test]
    public async Task GetLastCelebratedHolidaysReturnsOkForValidCountry()
    {
        // Arrange
        var cancellationToken = TestContext.CurrentContext.CancellationToken;
        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.Accenture_Assessment_AppHost>(cancellationToken);
        
        await using var app = await appHost.BuildAsync(cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);
        await app.StartAsync(cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);

        // Act
        var httpClient = app.CreateHttpClient("apiservice");
        await app.ResourceNotifications.WaitForResourceHealthyAsync("apiservice", cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);
        var response = await httpClient.GetAsync("/api/holidays/last-celebrated/US", cancellationToken);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var holidays = await response.Content.ReadFromJsonAsync<List<HolidayResultDto>>(cancellationToken);
        Assert.That(holidays, Is.Not.Null);
        Assert.That(holidays!.Count, Is.LessThanOrEqualTo(3));
    }

    [Test]
    public async Task GetLastCelebratedHolidaysReturnsBadRequestForInvalidCountryCode()
    {
        // Arrange
        var cancellationToken = TestContext.CurrentContext.CancellationToken;
        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.Accenture_Assessment_AppHost>(cancellationToken);
        
        await using var app = await appHost.BuildAsync(cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);
        await app.StartAsync(cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);

        // Act
        var httpClient = app.CreateHttpClient("apiservice");
        await app.ResourceNotifications.WaitForResourceHealthyAsync("apiservice", cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);
        var response = await httpClient.GetAsync("/api/holidays/last-celebrated/InvalidCode", cancellationToken);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task GetPublicHolidaysCountReturnsOkForValidInput()
    {
        // Arrange
        var cancellationToken = TestContext.CurrentContext.CancellationToken;
        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.Accenture_Assessment_AppHost>(cancellationToken);
        
        await using var app = await appHost.BuildAsync(cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);
        await app.StartAsync(cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);

        // Act
        var httpClient = app.CreateHttpClient("apiservice");
        await app.ResourceNotifications.WaitForResourceHealthyAsync("apiservice", cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);
        var response = await httpClient.GetAsync("/api/holidays/public-count/2024?countryCodes=US&countryCodes=CA", cancellationToken);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var counts = await response.Content.ReadFromJsonAsync<List<PublicHolidayCountDto>>(cancellationToken);
        Assert.That(counts, Is.Not.Null);
        Assert.That(counts!.Count, Is.EqualTo(2));
        Assert.That(counts.All(c => c.CountryCode.Length == 2), Is.True);
    }

    [Test]
    public async Task GetPublicHolidaysCountReturnsBadRequestForInvalidYear()
    {
        // Arrange
        var cancellationToken = TestContext.CurrentContext.CancellationToken;
        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.Accenture_Assessment_AppHost>(cancellationToken);
        
        await using var app = await appHost.BuildAsync(cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);
        await app.StartAsync(cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);

        // Act
        var httpClient = app.CreateHttpClient("apiservice");
        await app.ResourceNotifications.WaitForResourceHealthyAsync("apiservice", cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);
        var response = await httpClient.GetAsync("/api/holidays/public-count/999999?countryCodes=US", cancellationToken);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task GetPublicHolidaysCountReturnsBadRequestForMissingCountryCodes()
    {
        // Arrange
        var cancellationToken = TestContext.CurrentContext.CancellationToken;
        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.Accenture_Assessment_AppHost>(cancellationToken);
        
        await using var app = await appHost.BuildAsync(cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);
        await app.StartAsync(cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);

        // Act
        var httpClient = app.CreateHttpClient("apiservice");
        await app.ResourceNotifications.WaitForResourceHealthyAsync("apiservice", cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);
        var response = await httpClient.GetAsync("/api/holidays/public-count/2024", cancellationToken);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task GetPublicHolidaysCountResultsAreSortedDescending()
    {
        // Arrange
        var cancellationToken = TestContext.CurrentContext.CancellationToken;
        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.Accenture_Assessment_AppHost>(cancellationToken);
        
        await using var app = await appHost.BuildAsync(cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);
        await app.StartAsync(cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);

        // Act
        var httpClient = app.CreateHttpClient("apiservice");
        await app.ResourceNotifications.WaitForResourceHealthyAsync("apiservice", cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);
        var counts = await httpClient.GetFromJsonAsync<List<PublicHolidayCountDto>>(
            "/api/holidays/public-count/2024?countryCodes=US&countryCodes=CA&countryCodes=GB", 
            cancellationToken);

        // Assert
        Assert.That(counts, Is.Not.Null);
        Assert.That(counts!.Count, Is.EqualTo(3));
        
        // Verify descending order
        for (int i = 0; i < counts.Count - 1; i++)
        {
            Assert.That(counts[i].PublicHolidaysCount, Is.GreaterThanOrEqualTo(counts[i + 1].PublicHolidaysCount),
                $"Results should be sorted in descending order. {counts[i].CountryCode}({counts[i].PublicHolidaysCount}) should be >= {counts[i + 1].CountryCode}({counts[i + 1].PublicHolidaysCount})");
        }
    }

    [Test]
    public async Task GetSharedHolidaysReturnsOkForValidInput()
    {
        // Arrange
        var cancellationToken = TestContext.CurrentContext.CancellationToken;
        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.Accenture_Assessment_AppHost>(cancellationToken);
        
        await using var app = await appHost.BuildAsync(cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);
        await app.StartAsync(cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);

        // Act
        var httpClient = app.CreateHttpClient("apiservice");
        await app.ResourceNotifications.WaitForResourceHealthyAsync("apiservice", cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);
        var response = await httpClient.GetAsync("/api/holidays/shared/2024?countryCode1=US&countryCode2=CA", cancellationToken);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var sharedHolidays = await response.Content.ReadFromJsonAsync<List<SharedHolidayDto>>(cancellationToken);
        Assert.That(sharedHolidays, Is.Not.Null);
        
        // Verify each shared holiday has data for both countries
        foreach (var holiday in sharedHolidays!)
        {
            Assert.That(holiday.Country1Code, Is.EqualTo("US"));
            Assert.That(holiday.Country2Code, Is.EqualTo("CA"));
            Assert.That(holiday.Country1LocalName, Is.Not.Null.And.Not.Empty);
            Assert.That(holiday.Country2LocalName, Is.Not.Null.And.Not.Empty);
            Assert.That(holiday.Date, Is.Not.EqualTo(default(DateTime)));
        }
    }

    [Test]
    public async Task GetSharedHolidaysReturnsBadRequestForSameCountries()
    {
        // Arrange
        var cancellationToken = TestContext.CurrentContext.CancellationToken;
        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.Accenture_Assessment_AppHost>(cancellationToken);
        
        await using var app = await appHost.BuildAsync(cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);
        await app.StartAsync(cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);

        // Act
        var httpClient = app.CreateHttpClient("apiservice");
        await app.ResourceNotifications.WaitForResourceHealthyAsync("apiservice", cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);
        var response = await httpClient.GetAsync("/api/holidays/shared/2024?countryCode1=US&countryCode2=US", cancellationToken);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task GetSharedHolidaysResultsAreSortedByDate()
    {
        // Arrange
        var cancellationToken = TestContext.CurrentContext.CancellationToken;
        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.Accenture_Assessment_AppHost>(cancellationToken);
        
        await using var app = await appHost.BuildAsync(cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);
        await app.StartAsync(cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);

        // Act
        var httpClient = app.CreateHttpClient("apiservice");
        await app.ResourceNotifications.WaitForResourceHealthyAsync("apiservice", cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);
        var sharedHolidays = await httpClient.GetFromJsonAsync<List<SharedHolidayDto>>(
            "/api/holidays/shared/2024?countryCode1=US&countryCode2=CA", 
            cancellationToken);

        // Assert
        Assert.That(sharedHolidays, Is.Not.Null);
        
        if (sharedHolidays!.Count > 1)
        {
            // Verify ascending date order
            for (int i = 0; i < sharedHolidays.Count - 1; i++)
            {
                Assert.That(sharedHolidays[i].Date, Is.LessThanOrEqualTo(sharedHolidays[i + 1].Date),
                    "Shared holidays should be sorted by date in ascending order");
            }
        }
    }

    [Test]
    public async Task HealthCheckEndpointReturnsHealthy()
    {
        // Arrange
        var cancellationToken = TestContext.CurrentContext.CancellationToken;
        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.Accenture_Assessment_AppHost>(cancellationToken);
        
        await using var app = await appHost.BuildAsync(cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);
        await app.StartAsync(cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);

        // Act
        var httpClient = app.CreateHttpClient("apiservice");
        await app.ResourceNotifications.WaitForResourceHealthyAsync("apiservice", cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);
        var response = await httpClient.GetAsync("/health", cancellationToken);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        Assert.That(content, Does.Contain("Healthy"));
    }

    [Test]
    public async Task RateLimitingEnforcesLimits()
    {
        // Arrange
        var cancellationToken = TestContext.CurrentContext.CancellationToken;
        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.Accenture_Assessment_AppHost>(cancellationToken);
        
        await using var app = await appHost.BuildAsync(cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);
        await app.StartAsync(cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);

        // Act
        var httpClient = app.CreateHttpClient("apiservice");
        await app.ResourceNotifications.WaitForResourceHealthyAsync("apiservice", cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);
        
        var responses = new List<HttpResponseMessage>();
        
        // Make 101 requests rapidly (limit is 100 per minute)
        for (int i = 0; i < 101; i++)
        {
            var response = await httpClient.GetAsync("/api/countries", cancellationToken);
            responses.Add(response);
        }

        // Assert
        var tooManyRequestsResponses = responses.Count(r => r.StatusCode == HttpStatusCode.TooManyRequests);
        Assert.That(tooManyRequestsResponses, Is.GreaterThan(0), 
            "Rate limiting should reject some requests when limit is exceeded");
    }
}
