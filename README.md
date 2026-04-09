# DataIngestion

ASP.NET Core Web API for transaction ingestion with duplicate protection, customer transaction querying, and summary statistics.

## How to run

### Prerequisites

- .NET SDK 8.0+
- Docker (for local PostgreSQL or full stack)

### Option 1: Run with Docker Compose (recommended)

```bash
docker compose up --build
```

What this starts:
- `postgres` on host port `5433`
- `api` on `http://localhost:8080`

Swagger UI (in Development only) is available at:
- `https://localhost:7270/swagger/index.html`

### Option 2: Run API locally + PostgreSQL in Docker

1. Start only database:
```bash
docker compose up postgres -d
```

2. Run API:
```bash
dotnet run --project DataIngestion.Api
```

Default local DB connection (from `DataIngestion.Api/appsettings.json`):
- `Host=localhost;Port=5433;Database=dataingestiondb;Username=postgres;Password=postgres`

### Run tests

```bash
dotnet test DataIngestion.Application.Tests/DataIngestion.Application.Tests.csproj
```

Or for all projects in solution:
```bash
dotnet test DataIngestion.slnx
```

## Serilog Logs

The project is configured with two Serilog sinks:
- console output: `WriteTo.Console()`
- file output: `WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)`

Log files are written to the `logs` folder with daily rolling (`log-YYYYMMDD.txt`).

Where to find the `logs` folder:
- if you run from `DataIngestion.Api` (`cd DataIngestion.Api && dotnet run`) -> `DataIngestion.Api/logs`
- if you run from repo root (`dotnet run --project DataIngestion.Api`) -> `logs` in repo root

## API Test case
1. Use test1.csv for batch ingest
result 2 accepted , 3 rejected

## Architecture description

The solution follows a layered architecture with clear responsibilities:

- `DataIngestion.Api`
  - HTTP layer: controllers, middleware, DI registration.
  - Endpoints:
    - `POST /api/ingest/transaction`
    - `POST /api/ingest/batch`
    - `GET /api/customers/{customerId}/transactions`
    - `GET /api/statistics/summary`
- `DataIngestion.Application`
  - Use-case/business orchestration services.
  - Contracts (`ITransactionRepository`, service interfaces), DTOs, validators.
  - Key services:
    - `TransactionIngestionService`
    - `BatchIngestionService`
    - `CustomerTransactionQueryService`
    - `StatsQueryService`
- `DataIngestion.Domain`
  - Core entities and domain model (`Transaction`).
- `DataIngestion.Infrastructure`
  - EF Core `AppDbContext`, repository implementation, persistence configs.

Key runtime flow:
1. API controller receives request and forwards to application service.
2. Application service validates input, applies business rules (including deduplication key checks).
3. Repository persists/queries via EF Core + PostgreSQL.
4. API returns DTO response.

Notes:
- DB migrations are applied automatically on startup (`Program.cs`).
- Logging is handled with Serilog (console + rolling files under `logs/`).

## Trade-offs considered

- **Layered design over minimal APIs**
  - Pros: clear separation, easier unit testing of use-cases.
  - Cons: more files/boilerplate.

- **Deduplication at application-service level**
  - Pros: explicit behavior and domain-friendly error handling.
  - Cons: requires extra query before insert; race conditions still possible without strict DB constraints.

- **Auto-migration on startup**
  - Pros: easier local/dev startup.
  - Cons: less control for production rollout and migration timing.

- **CSV ingestion in service**
  - Pros: fast to deliver and easy to follow in one place.
  - Cons: parsing, validation, persistence concerns are coupled unless further modularized.

## What I would do differently with more time

- Introduce integration tests with Testcontainers (PostgreSQL) for repository and migration behavior.
- Improve batch ingestion throughput (buffered writes / transaction batching / bulk insert strategy).
- Add richer operational docs (failure handling, retry semantics, observability dashboards).


## AI Tools Usage

- **Which tools did you use and for what?**
    - ChatGPT – used for naming suggestions, generating Docker-related files (Dockerfile, docker-compose), and general guidance.
    - Codex – used for writing unit tests and assisting with code refactoring.

- **What did you accept as-is, modify, or write from scratch?**
  - Accepted as-is: some boilerplate code and initial suggestions from ChatGPT (e.g., basic Docker setup).
  - Modified: generated code (Dockerfiles, tests) to better fit project requirements and structure.
  - Written from scratch: unit tests.


- **Did the AI get anything wrong? How did you catch it?**
  - Yes, occasionally the AI produced incomplete or slightly incorrect code (e.g., missing configurations or edge cases).
  These issues were identified through:
     1. manual code review,
     2. running the application and tests,
debugging and validating expected behavior.