***English** | [Portugues](README.md)*

# RVM.LogStream

Centralized log ingestion and search platform with configurable retention, SignalR dashboard and analytics.

![build](https://img.shields.io/badge/build-passing-brightgreen)
![tests](https://img.shields.io/badge/tests-44%20passed-brightgreen)
![license](https://img.shields.io/badge/license-MIT-blue)
![dotnet](https://img.shields.io/badge/.NET-10.0-purple)

---

## About

RVM.LogStream is a centralized log platform that supports batch ingestion, advanced search with multiple filters (source, level, query, correlationId, date range), configurable retention policies per source, and volume analytics by level/source. The system uses SignalR for real-time push of new logs and provides a complete observability pipeline.

---

## Technologies

| Layer | Stack |
|---|---|
| Runtime | .NET 10, ASP.NET Core 10 |
| Real-time | SignalR |
| ORM | Entity Framework Core 10 |
| Database | PostgreSQL (Npgsql 10.0.1) |
| Logging | Serilog + Compact JSON |
| Authentication | API Key (header `X-API-Key`) |
| Testing | xUnit 2.9, Moq 4.20, EF Core InMemory |
| Containerization | Docker, Docker Compose |

---

## Architecture

```
┌───────────────────────────────────────────────────────┐
│                     API Layer                         │
│  ┌─────────────┐ ┌──────────┐ ┌───────────────────┐  │
│  │  Ingestion  │ │  Search  │ │  Retention/Stats  │  │
│  │  Controller │ │Controller│ │   Controllers     │  │
│  └──────┬──────┘ └────┬─────┘ └────────┬──────────┘  │
│         │             │                │              │
│  ┌──────▼──────┐ ┌────▼─────┐  ┌───────▼──────────┐  │
│  │  Ingestion  │ │  Search  │  │  RetentionWorker  │  │
│  │  Service    │ │  Service │  │ (BackgroundService)│  │
│  └──────┬──────┘ └────┬─────┘  └───────┬──────────┘  │
│         │             │                │              │
│  ┌──────▼─────────────▼────────────────▼──────────┐  │
│  │              SignalR Hub (Push)                 │  │
│  └────────────────────┬───────────────────────────┘  │
├───────────────────────┼───────────────────────────────┤
│                 Domain Layer                          │
│  ┌────────────┐ ┌───────────┐ ┌───────────────────┐  │
│  │  LogEntry  │ │ LogSource │ │ RetentionPolicy   │  │
│  └────────────┘ └───────────┘ └───────────────────┘  │
│  ┌────────────────────────────────────────────────┐  │
│  │           Repository Interfaces                │  │
│  └────────────────────┬───────────────────────────┘  │
├───────────────────────┼───────────────────────────────┤
│             Infrastructure Layer                      │
│  ┌────────────────────▼───────────────────────────┐  │
│  │         LogStreamDbContext (EF Core)            │  │
│  │         PostgreSQL + Npgsql                     │  │
│  └────────────────────────────────────────────────┘  │
│  ┌────────────────────────────────────────────────┐  │
│  │         Repository Implementations             │  │
│  └────────────────────────────────────────────────┘  │
└───────────────────────────────────────────────────────┘
```

---

## Project Structure

```
RVM.LogStream/
├── src/
│   ├── RVM.LogStream.API/
│   │   ├── Auth/
│   │   │   ├── ApiKeyAuthHandler.cs
│   │   │   └── ApiKeyAuthOptions.cs
│   │   ├── Controllers/
│   │   │   ├── IngestionController.cs
│   │   │   ├── RetentionController.cs
│   │   │   ├── SearchController.cs
│   │   │   ├── SourcesController.cs
│   │   │   └── StatsController.cs
│   │   ├── Dtos/
│   │   │   ├── IngestionDtos.cs
│   │   │   ├── RetentionDtos.cs
│   │   │   ├── SearchDtos.cs
│   │   │   ├── SourceDtos.cs
│   │   │   └── StatsDtos.cs
│   │   ├── Health/
│   │   │   └── DatabaseHealthCheck.cs
│   │   ├── Hubs/
│   │   │   └── LogStreamHub.cs
│   │   ├── Middleware/
│   │   │   └── CorrelationIdMiddleware.cs
│   │   ├── Services/
│   │   │   ├── LogIngestionService.cs
│   │   │   ├── LogSearchService.cs
│   │   │   └── RetentionWorker.cs
│   │   ├── Program.cs
│   │   └── appsettings.json
│   ├── RVM.LogStream.Domain/
│   │   ├── Entities/
│   │   │   ├── LogEntry.cs
│   │   │   ├── LogSource.cs
│   │   │   └── RetentionPolicy.cs
│   │   ├── Enums/
│   │   │   └── LogLevel.cs
│   │   ├── Interfaces/
│   │   │   ├── ILogEntryRepository.cs
│   │   │   ├── ILogSourceRepository.cs
│   │   │   └── IRetentionPolicyRepository.cs
│   │   └── Models/
│   │       └── AggregationModels.cs
│   └── RVM.LogStream.Infrastructure/
│       ├── Data/
│       │   ├── Configurations/
│       │   │   ├── LogEntryConfiguration.cs
│       │   │   ├── LogSourceConfiguration.cs
│       │   │   └── RetentionPolicyConfiguration.cs
│       │   └── LogStreamDbContext.cs
│       ├── Repositories/
│       │   ├── LogEntryRepository.cs
│       │   ├── LogSourceRepository.cs
│       │   └── RetentionPolicyRepository.cs
│       └── DependencyInjection.cs
├── test/
│   └── RVM.LogStream.Test/
│       ├── Domain/
│       │   └── EntityTests.cs
│       ├── Infrastructure/
│       │   ├── LogEntryRepositoryTests.cs
│       │   ├── LogSourceRepositoryTests.cs
│       │   └── RetentionPolicyRepositoryTests.cs
│       └── Services/
│           └── LogIngestionServiceTests.cs
├── docker-compose.dev.yml
├── docker-compose.prod.yml
└── RVM.LogStream.slnx
```

---

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [PostgreSQL](https://www.postgresql.org/) (or Docker)

### Configuration

1. Clone the repository:
```bash
git clone https://github.com/rvenegas5/RVM.LogStream.git
cd RVM.LogStream
```

2. Configure the connection string and API keys via environment variables or `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=rvmlogstream;Username=postgres;Password=your_password"
  },
  "ApiKeys": {
    "Keys": [
      { "Key": "your-api-key", "AppId": "app-1", "Name": "MyApp" }
    ]
  },
  "Retention": {
    "CheckIntervalMinutes": 60
  }
}
```

3. Run the application:
```bash
cd src/RVM.LogStream.API
dotnet run
```

### Docker Compose

```bash
docker compose -f docker-compose.dev.yml up -d
```

The API will be available at `http://localhost:8080`.

---

## API Endpoints

All endpoints require authentication via the `X-API-Key` header.

### Ingestion

| Method | Route | Description |
|---|---|---|
| `POST` | `/api/ingestion` | Batch log ingestion |

**Request body:**
```json
{
  "events": [
    {
      "timestamp": "2026-04-09T12:00:00Z",
      "level": "Error",
      "message": "Connection failed",
      "messageTemplate": null,
      "source": "payments-api",
      "correlationId": "abc-123",
      "properties": "{\"key\":\"value\"}",
      "exception": "NullReferenceException"
    }
  ]
}
```

**Response:** `{ "accepted": 1, "rejected": 0 }`

### Search

| Method | Route | Description |
|---|---|---|
| `GET` | `/api/search` | Search with advanced filters |

**Query parameters:** `query`, `source`, `level`, `correlationId`, `from`, `to`, `offset`, `limit`

### Sources

| Method | Route | Description |
|---|---|---|
| `GET` | `/api/sources` | List all sources |
| `GET` | `/api/sources/{name}` | Get source by name |

### Retention

| Method | Route | Description |
|---|---|---|
| `GET` | `/api/retention` | List retention policies |
| `GET` | `/api/retention/{id}` | Get policy by ID |
| `POST` | `/api/retention` | Create retention policy |
| `PUT` | `/api/retention/{id}` | Update policy |
| `DELETE` | `/api/retention/{id}` | Delete policy |

### Statistics

| Method | Route | Description |
|---|---|---|
| `GET` | `/api/stats` | Volume by level and by source |

**Query parameters:** `source`, `from`, `to`

### Real-time

| Protocol | Route | Description |
|---|---|---|
| SignalR | `/hubs/log-stream` | Real-time push of new logs |

**Events:** `LogReceived` | **Groups:** `JoinSourceGroup(source)`, `LeaveSourceGroup(source)`

### Health Check

| Method | Route | Description |
|---|---|---|
| `GET` | `/health` | Health check (public) |

---

## Tests

The project has **44 tests** covering domain, infrastructure and services.

```bash
dotnet test
```

| Suite | File | Tests |
|---|---|---|
| Domain | `EntityTests.cs` | 12 (6 Fact + 1 Theory x6 InlineData) |
| Infrastructure | `LogEntryRepositoryTests.cs` | 12 |
| Infrastructure | `LogSourceRepositoryTests.cs` | 5 |
| Infrastructure | `RetentionPolicyRepositoryTests.cs` | 8 |
| Services | `LogIngestionServiceTests.cs` | 7 |
| **Total** | | **44** |

### Testing stack
- **xUnit 2.9** - Testing framework
- **Moq 4.20** - Mocking for services
- **EF Core InMemory** - In-memory database for repository tests

---

## Features

- **Batch ingestion** - Send multiple log entries in a single request
- **Advanced search** - Filters by source, level, query, correlationId and date range
- **Retention policies** - Configurable retention per source with pattern matching
- **RetentionWorker** - BackgroundService that automatically applies retention policies
- **Volume analytics** - Log aggregation by level and by source
- **SignalR push** - Real-time notification of new logs via WebSocket
- **6 log levels** - Trace, Debug, Information, Warning, Error, Fatal
- **Pagination** - Offset/limit with clamp (max 200 per page)
- **Correlation ID** - Middleware that propagates or generates X-Correlation-ID
- **Rate limiting** - 60 requests/minute per IP
- **API Key auth** - Key-based authentication with app identification
- **Health check** - `/health` endpoint with database connectivity verification
- **Docker ready** - Docker Compose for dev and prod with `rvmtech` network

---

<p align="center">
  Built by <strong>RVM Tech</strong>
</p>
