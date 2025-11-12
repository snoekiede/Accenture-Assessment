# Accenture Assessment - Holiday Information System

A production-ready .NET 9 application built with Blazor Server, ASP.NET Core Minimal APIs, and .NET Aspire for cloud-native orchestration. This application provides comprehensive holiday information across multiple countries with intelligent caching, rate limiting, and robust error handling.

## Table of Contents

- [Overview](#overview)
- [Features](#features)
- [Architecture](#architecture)
- [Technology Stack](#technology-stack)
- [Prerequisites](#prerequisites)
- [Getting Started](#getting-started)
- [Running the Application](#running-the-application)
- [Running Tests](#running-tests)
- [API Documentation](#api-documentation)
- [Project Structure](#project-structure)
- [Configuration](#configuration)
- [Disclaimer](#disclaimer)
- [License](#license)

---

## Overview

This application demonstrates enterprise-grade .NET development practices by providing a holiday information service that:

- Fetches and caches holiday data from external APIs
- Provides a user-friendly Blazor web interface
- Offers RESTful API endpoints for programmatic access
- Implements production-ready features (caching, rate limiting, health checks, logging)
- Uses .NET Aspire for simplified cloud-native application orchestration

### Key Capabilities

1. **Last Celebrated Holidays** - View the most recent holidays celebrated in any country
2. **Public Holiday Count** - Compare public holidays (excluding weekends) across multiple countries
3. **Shared Holidays** - Discover holidays celebrated on the same date in different countries
4. **Country Information** - Browse all available countries with holiday data

---

## Features

### Production-Ready Features

- **CORS Configuration** - Secure cross-origin resource sharing
- **Rate Limiting** - 100 requests per minute per client
- **Output Caching** - Redis-backed distributed caching with strategic expiration
- **Input Validation** - Comprehensive request validation with descriptive errors
- **Retry Policies** - Exponential backoff for external API calls
- **Health Checks** - Database and external API monitoring
- **Structured Logging** - Context-rich log messages
- **Database Migrations** - EF Core migrations for schema versioning
- **Error Handling** - Graceful error handling with proper HTTP status codes
- **OpenAPI/Swagger** - Comprehensive API documentation

### Business Features

- **Multi-Country Support** - Access holiday data for 100+ countries
- **Historical Data** - Query past and future holidays
- **Smart Filtering** - Filter by date, country, and holiday type
- **Comparative Analysis** - Compare holiday counts across countries
- **Responsive UI** - Modern Blazor Server interface with real-time updates

---


### Project Structure

- **Accenture-Assessment.Web** - Blazor Server frontend application
- **Accenture-Assessment.ApiService** - ASP.NET Core Minimal API backend
- **Accenture-Assessment.AppHost** - .NET Aspire orchestration project
- **Accenture-Assessment.Data** - Data access layer (EF Core, repositories, services)
- **Accenture-Assessment.Contracts** - Shared DTOs and contracts
- **Accenture-Assessment.ServiceDefaults** - Shared Aspire service configurations
- **Accenture-Assessment.Tests** - Comprehensive test suite (68 tests)

---

## Technology Stack

| Category | Technology | Version |
|----------|-----------|---------|
| **Framework** | .NET | 9.0 |
| **Frontend** | Blazor Server | 9.0 |
| **Backend** | ASP.NET Core Minimal APIs | 9.0 |
| **Database** | SQL Server | 2022+ |
| **ORM** | Entity Framework Core | 9.0 |
| **Cache** | Redis | Latest |
| **Orchestration** | .NET Aspire | 9.5.0 |
| **Testing** | NUnit | 4.4.0 |
| **Mocking** | Moq | 4.20.72 |
| **Resilience** | Polly | via Aspire |
| **External API** | Nager.Date API | v3 |

---

## Prerequisites

Before running this application, ensure you have the following installed:

### Required

- **.NET 9 SDK** - [Download](https://dotnet.microsoft.com/download/dotnet/9.0)
- **Docker Desktop** - [Download](https://www.docker.com/products/docker-desktop) (for SQL Server and Redis containers)
- **.NET Aspire workload** - Install with: `dotnet workload install aspire`

### Optional

- **Visual Studio 2022 (v17.13+)** - With Aspire workload
- **JetBrains Rider 2024.3+** - With Aspire support
- **Visual Studio Code** - With C# Dev Kit extension

### Verify Installation

```bash
# Check .NET version
dotnet --version  # Should be 9.0.x or higher

# Check Aspire workload
dotnet workload list  # Should show 'aspire'

# Check Docker
docker --version
docker-compose --version
```

---

## Getting Started

### 1. Clone the Repository

```bash
git clone https://github.com/snoekiede/Accenture-Assessment.git
cd Accenture-Assessment
```

### 2. Restore Dependencies

```bash
dotnet restore
```

### 3. Build the Solution

```bash
dotnet build
```

### 4. Apply Database Migrations

The application will automatically apply migrations on startup in development mode. However, you can also apply them manually:

```bash
dotnet ef database update --project Accenture-Assessment.Data --startup-project Accenture-Assessment.ApiService
```

---

## ?? Running the Application

### Option 1: Using .NET Aspire (Recommended)

This is the easiest way to run the application as it automatically starts all dependencies (SQL Server, Redis) in Docker containers.

```bash
# From the solution root
dotnet run --project Accenture-Assessment.AppHost
```

**What happens:**
1. Aspire Dashboard starts at `http://localhost:15000` (or similar)
2. SQL Server container starts
3. Redis container starts
4. API Service starts at `https://localhost:7001`
5. Web Frontend starts at `https://localhost:7148`

**Access the application:**
- **Web UI**: https://localhost:7148
- **Aspire Dashboard**: http://localhost:15000
- **API (Swagger)**: https://localhost:7001/openapi (development only)
- **Health Check**: https://localhost:7001/health

### Option 2: Using Visual Studio

1. Open `Accenture-Assessment.sln` in Visual Studio
2. Set `Accenture-Assessment.AppHost` as the startup project
3. Press F5 or click "Start"

### Option 3: Using Docker Compose (Manual Setup)

If you prefer not to use Aspire:

```bash
# Start dependencies
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=YourStrong@Passw0rd" -p 1433:1433 -d mcr.microsoft.com/mssql/server:2022-latest
docker run -p 6379:6379 -d redis:latest

# Update connection strings in appsettings.Development.json

# Run API Service
dotnet run --project Accenture-Assessment.ApiService

# Run Web Frontend (in another terminal)
dotnet run --project Accenture-Assessment.Web
```

---

## Running Tests

The solution includes **47 comprehensive tests** covering unit, integration, and constraint testing.

### Run All Tests

```bash
dotnet test
```

### Run Specific Test Categories

```bash
# Unit tests only
dotnet test --filter "FullyQualifiedName~RepositoryTests"


# Service layer tests
dotnet test --filter "FullyQualifiedName~HolidayDataServiceTests"

# Constraint tests (SQLite)
dotnet test --filter "FullyQualifiedName~ConstraintTests"
```

### Run with Detailed Output

```bash
dotnet test --logger "console;verbosity=detailed"
```

### Test Coverage

| Test Suite | Tests | Coverage |
|------------|-------|----------|
| **CountryRepositoryTests** | 18 | Unit tests (InMemory) |
| **CountryRepositoryConstraintTests** | 3 | Constraint tests (SQLite) |
| **HolidayRepositoryTests** | 26 | Unit tests (InMemory) |
| **HolidayDataServiceTests** | 7 | Unit tests (Mocked) |
| **Total** | **68** | **100% method coverage** |

---

## API Documentation

### Base URL

```
https://localhost:7001/api
```

### Endpoints

#### 1. Get All Countries

```http
GET /api/countries?forceSync=false
```

**Response:**
```json
[
  {
    "countryCode": "US",
    "name": "United States"
  }
]
```

#### 2. Get Last Celebrated Holidays

```http
GET /api/holidays/last-celebrated/{countryCode}
```

**Example:**
```http
GET /api/holidays/last-celebrated/US
```

**Response:**
```json
[
  {
    "date": "2024-12-25T00:00:00",
    "name": "Christmas Day",
    "localName": "Christmas Day"
  }
]
```

#### 3. Get Public Holidays Count

```http
GET /api/holidays/public-count/{year}?countryCodes={code1}&countryCodes={code2}
```

**Example:**
```http
GET /api/holidays/public-count/2024?countryCodes=US&countryCodes=CA&countryCodes=GB
```

**Response:**
```json
[
  {
    "countryCode": "GB",
    "publicHolidaysCount": 8
  },
  {
    "countryCode": "US",
    "publicHolidaysCount": 11
  },
  {
    "countryCode": "CA",
    "publicHolidaysCount": 5
  }
]
```

#### 4. Get Shared Holidays

```http
GET /api/holidays/shared/{year}?countryCode1={code1}&countryCode2={code2}
```

**Example:**
```http
GET /api/holidays/shared/2024?countryCode1=US&countryCode2=CA
```

**Response:**
```json
[
  {
    "date": "2024-12-25T00:00:00",
    "country1Code": "US",
    "country1LocalName": "Christmas Day",
    "country2Code": "CA",
    "country2LocalName": "Christmas Day"
  }
]
```

#### 5. Health Check

```http
GET /health
```

**Response:**
```json
{
  "status": "Healthy",
  "totalDuration": "00:00:00.0234567",
  "entries": {
    "database": {
      "status": "Healthy"
    },
    "nager-api": {
      "status": "Healthy"
    }
  }
}
```

For complete API documentation, visit the Swagger UI when running in development mode:
?? https://localhost:7001/openapi

---



## Configuration

### appsettings.json

```json
{
  "ExternalApis": {
    "NagerDate": {
      "BaseUrl": "https://date.nager.at/api/v3/",
      "MaxRetries": 3,
      "RetryDelaySeconds": 2
    }
  },
  "AllowedOrigins": "https://localhost:7148",
  "RateLimiting": {
    "PermitLimit": 100,
    "Window": "00:01:00"
  },
  "Validation": {
    "MinYear": 1900,
    "MaxYear": 2100
  }
}
```

### Environment Variables

| Variable | Description | Default |
|----------|-------------|---------|
| `ASPNETCORE_ENVIRONMENT` | Runtime environment | `Development` |
| `ConnectionStrings__holidaysdb` | SQL Server connection | Managed by Aspire |
| `ConnectionStrings__cache` | Redis connection | Managed by Aspire |

---

## Disclaimer

### Development & Testing Only

**This application is provided as-is for educational and demonstration purposes.**

- **Not Production-Deployed**: While the code includes production-ready features, this specific instance is a development prototype
- **External API Dependency**: Relies on the free [Nager.Date API](https://date.nager.at/) which may have rate limits or availability issues
- **No Warranty**: No guarantees regarding accuracy, availability, or fitness for any particular purpose
- **Data Accuracy**: Holiday data is sourced from external APIs and may not be 100% accurate or up-to-date
- **Authentication**: Currently has no authentication - add authentication/authorization before public deployment

### Security Considerations

Before deploying to production, ensure you:

1. Add authentication (JWT, OAuth2, etc.)
2. Configure HTTPS with proper certificates
3. Secure connection strings (use Azure Key Vault or similar)
4. Review and harden CORS policies
5. Implement proper logging and monitoring
6. Add API key management if exposing publicly
7. Review rate limiting settings for expected load
8. Test with production-like data volumes

### External Dependencies

This application depends on:

- **Nager.Date API** - Free public API for holiday data
  - No SLA or availability guarantees
  - Subject to rate limiting
  - May change or be discontinued
- **Docker containers** - SQL Server and Redis
  - Requires Docker Desktop to be running
  - Containers managed by .NET Aspire

### License Compatibility

- .NET 9 - MIT License
- Entity Framework Core - MIT License
- Blazor - MIT License
- External API - Check [Nager.Date terms](https://date.nager.at/)

---



---

## Contributing

This is an assessment project and is not accepting contributions. However, feel free to fork and modify for your own learning purposes.

---

## Contact

For questions about this assessment project, please contact:

- **GitHub**: [@snoekiede](https://github.com/snoekiede)
- **Repository**: [Accenture-Assessment](https://github.com/snoekiede/Accenture-Assessment)

---

## Learning Objectives Demonstrated

This project demonstrates:

? **Clean Architecture** - Separation of concerns with proper layering  
? **SOLID Principles** - Dependency injection, single responsibility  
? **.NET 9 Features** - Latest framework capabilities  
? **Blazor Development** - Modern web UI with .NET  
? **Minimal APIs** - Lightweight, performant API design  
? **Entity Framework Core** - Database access with migrations  
? **Aspire Orchestration** - Cloud-native application development  
? **Production Patterns** - Caching, rate limiting, health checks  
? **Comprehensive Testing** - 68 tests with 100% method coverage  
? **Error Handling** - Graceful failures and proper status codes  
? **API Integration** - External API consumption with resilience  
? **Documentation** - Complete project documentation  

---

## Project Stats

- **Lines of Code**: ~5,000
- **Test Coverage**: 100% method coverage
- **Total Tests**: 41 (all passing)
- **Projects**: 7
- **External APIs**: 1 (Nager.Date)
- **Database Tables**: 2 (Countries, Holidays)
- **API Endpoints**: 5
- **Production Features**: 15+

---

## Quick Start Summary

```bash
# 1. Clone
git clone https://github.com/snoekiede/Accenture-Assessment.git
cd Accenture-Assessment

# 2. Install .NET 9 and Aspire workload
dotnet workload install aspire

# 3. Run
dotnet run --project Accenture-Assessment.AppHost

# 4. Open browser
https://localhost:7148
```

**That's it!** The application will:
- Start SQL Server in Docker
- Start Redis in Docker
- Apply database migrations
- Start API service
- Start web frontend
- Open Aspire Dashboard

---

## Star This Project

If you found this project helpful for learning .NET 9, Blazor, or .NET Aspire, please consider giving it a star on GitHub!

---

**Made using .NET 9 and .NET Aspire**
