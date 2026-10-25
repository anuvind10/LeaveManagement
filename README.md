# Leave Management System

A production-style REST API built with **.NET 10** and **Clean Architecture**, designed to manage employee leave requests in an enterprise environment. Built as a learning project to demonstrate modern backend engineering practices including JWT authentication, role-based authorization, optimistic concurrency, domain-driven design, and containerized deployment.

---

## Tech Stack

| Concern | Technology |
|---|---|
| Runtime | .NET 10 |
| Web Framework | ASP.NET Core Web API |
| ORM | Entity Framework Core 10 |
| Database | SQL Server 2022 |
| Authentication | JWT Bearer Tokens |
| Identity | ASP.NET Core Identity |
| Validation | FluentValidation |
| Mapping | Mapperly (source generator) |
| Logging | Serilog |
| API Docs | Swagger / OpenAPI |
| API Versioning | Asp.Versioning |
| Testing | xUnit + Moq |
| Containerization | Docker + Docker Compose |
| CI/CD | GitHub Actions → GitHub Container Registry → Azure App Service |
| Observability | Application Insights + Serilog sink |

---

## Architecture

Four-layer Clean Architecture with strict unidirectional dependency flow:

```
API  ──────────────────────────────────────────────►  Application  ──►  Domain
Infrastructure  ──────────────────────────────────►  Application  ──►  Domain
```

| Layer | Project | Responsibility |
|---|---|---|
| Domain | `LeaveManagement.Domain` | Entities, enums, domain exceptions, business rules |
| Application | `LeaveManagement.Application` | Interfaces, DTOs, services, validators, mappings |
| Infrastructure | `LeaveManagement.Infrastructure` | EF Core, repositories, token service, migrations |
| API | `LeaveManagement.API` | Controllers, middleware, current user service, startup |
| Tests | `LeaveManagement.Tests` | Unit tests (domain + application layer) |

The Domain and Application layers have **zero dependencies on ASP.NET Core or Entity Framework Core** — they depend only on abstractions.

---

## Features

- **Submit** leave requests with FluentValidation on dates and leave type
- **Approve / Reject / Cancel** with full audit trail per request
- **Role-based authorization** — Employee, Manager, HR roles with different access levels
- **Pagination, filtering, and sorting** on all list endpoints
- **Rich domain model** — business rules live on the entity, not in services
- **Optimistic concurrency** via SQL Server `rowversion` — prevents lost updates in concurrent environments
- **Global exception middleware** — consistent JSON error responses across all error types
- **JWT authentication** with claims-based identity
- **Structured logging** with Serilog — UserId and UserName enriched on every log entry
- **Health checks** — separate liveness and readiness probes
- **API versioning** — URL segment strategy, Swagger auto-discovers versions
- **Dockerized** — multi-stage build, Docker Compose with SQL Server

---

## Domain Model

### Leave Request State Machine

```
            ┌────────────────────────────────────────┐
            │                                        │
            ▼                                        │
        [Pending] ──► Approved ──► Canceled          │
            │                                        │
            └──► Rejected                            │
            │                                        │
            └──► Canceled ───────────────────────────┘
```

- Only **Pending** requests can be Approved or Rejected
- Both **Pending** and **Approved** requests can be Canceled
- Every state transition creates an immutable `LeaveAudit` record

### Roles and Permissions

| Action | Employee | Manager | HR |
|---|---|---|---|
| Submit leave request | ✅ (own) | ✅ (own) | ✅ (own) |
| View leave request | ✅ (own only) | ✅ (all) | ✅ (all) |
| List all requests | ❌ | ✅ | ✅ |
| List by employee | ✅ (own only) | ✅ (any) | ✅ (any) |
| Approve / Reject | ❌ | ✅ | ❌ |
| Cancel | ✅ (own) | ✅ | ✅ |

---

## API Endpoints

All endpoints require a valid JWT token unless noted. Base path: `/api/v1`

| Method | Route | Role | Description |
|---|---|---|---|
| `POST` | `/api/auth` | Public | Login — returns JWT token |
| `POST` | `/api/v1/leaverequest` | Any | Submit a leave request |
| `GET` | `/api/v1/leaverequest/{id}` | Any | Get by ID (ownership enforced) |
| `GET` | `/api/v1/leaverequest` | Manager, HR | Get all with filter/sort/pagination |
| `GET` | `/api/v1/leaverequest/employee/{employeeId}` | Any | Get by employee with filter/sort/pagination |
| `PUT` | `/api/v1/leaverequest/{id}/approve` | Manager | Approve a pending request |
| `PUT` | `/api/v1/leaverequest/{id}/reject` | Manager | Reject a pending request |
| `PUT` | `/api/v1/leaverequest/{id}/cancel` | Any | Cancel a pending or approved request |
| `GET` | `/health/live` | Public | Liveness probe |
| `GET` | `/health/ready` | Public | Readiness probe (checks DB) |

### Query Parameters (list endpoints)

| Parameter | Type | Default | Description |
|---|---|---|---|
| `page` | int | 1 | Page number |
| `pageSize` | int | 10 | Results per page (max 100) |
| `field` | enum | SubmittedDate | Sort field: `SubmittedDate`, `NoOfDays` |
| `direction` | enum | Ascending | Sort direction: `Ascending`, `Descending` |
| `status` | enum | — | Filter: `Pending`, `Approved`, `Rejected`, `Canceled` |
| `type` | enum | — | Filter: `Annual`, `Sick`, `Unpaid`, `Maternity`, `Paternity` |
| `fromDate` | datetime | — | Filter by submitted date (inclusive) |
| `toDate` | datetime | — | Filter by submitted date (inclusive) |

---

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (for Docker Compose setup)
- SQL Server (if running locally without Docker)

### Option 1 — Docker Compose (recommended)

```bash
# Clone the repo
git clone https://github.com/your-username/leave-management-api.git
cd leave-management-api

# Create a .env file from the template below
docker compose up --build
```

The API will be available at `http://localhost:8080/swagger`.

**.env file template:**

```env
ConnectionStrings__DefaultConnection=Server=db;Database=LeaveManagementDb;User Id=sa;Password=YourStrong@Password;TrustServerCertificate=True;
JwtSettings__SecretKey=your-super-secret-key-that-is-at-least-32-chars
JwtSettings__Issuer=LeaveManagementAPI
JwtSettings__Audience=LeaveManagementClient
JwtSettings__ExpiryInMinutes=60
ASPNETCORE_ENVIRONMENT=Development
ACCEPT_EULA=Y
SA_PASSWORD=YourStrong@Password
```

### Option 2 — Local Development (DB in Docker, API on host)

SQL Server is not installed locally — it runs in a Docker container. The API runs on the host via `dotnet run`.

**Step 1** — Start the database container:

```cmd
docker compose up db -d
```

**Step 2** — Create `LeaveManagement.API/appsettings.Development.json` (gitignored — recreate manually on each machine):

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=LeaveManagementDb;User Id=sa;Password=YourStrong@Password;TrustServerCertificate=true;"
  },
  "JwtSettings": {
    "SecretKey": "your-super-secret-key-that-is-at-least-32-chars"
  }
}
```

> **Note:** Use `User Id=sa` (SQL auth) — not `Trusted_Connection=true` (Windows auth). The Docker SQL Server runs on Linux and does not support Windows Authentication. The comma in `localhost,1433` is the SQL Server syntax for specifying a non-default port.

**Step 3** — Run the API:

```cmd
dotnet restore
dotnet run --project LeaveManagement.API
```

Navigate to: `https://localhost:62978/swagger`

Migrations run automatically on startup.

### Seeded Test Users (Development only)

| Email | Password | Role |
|---|---|---|
| employee@test.com | Test@1234 | Employee |
| manager@test.com | Test@1234 | Manager |
| hr@test.com | Test@1234 | HR |

---

## Running Tests

```bash
dotnet test
dotnet test --verbosity normal    # shows individual test names and results
```

**34 tests** across three test classes:

| Test Class | Count | Covers |
|---|---|---|
| `LeaveRequestTests` | 11 | Domain entity state transitions and audit creation |
| `CreateLeaveRequestValidatorTests` | 4 | FluentValidation rules |
| `LeaveRequestServiceTest` | 19 | Application service logic with mocked dependencies |

---

## Database Migrations

```bash
# Add a new migration
dotnet ef migrations add <MigrationName> \
  --project LeaveManagement.Infrastructure \
  --startup-project LeaveManagement.API

# Apply migrations manually
dotnet ef database update \
  --project LeaveManagement.Infrastructure \
  --startup-project LeaveManagement.API

# Remove last unapplied migration
dotnet ef migrations remove \
  --project LeaveManagement.Infrastructure \
  --startup-project LeaveManagement.API
```

---

## Project Structure

```
LeaveManagement.slnx
├── LeaveManagement.Domain/
│   ├── Entities/          # LeaveRequest, LeaveAudit, ApplicationUser
│   ├── Enums/             # LeaveType, LeaveStatus, LeaveAction
│   └── Exceptions/        # DomainException, InvalidLeaveStatusException
│
├── LeaveManagement.Application/
│   ├── Common/            # Pagination, sort, filter param models
│   ├── DTOs/              # Request/response data transfer objects
│   ├── Enums/             # SortByField, SortDirection
│   ├── Exceptions/        # ValidationException, ForbiddenAccessException, ConcurrencyException
│   ├── Interfaces/        # ILeaveRequestRepository, ILeaveRequestService, ITokenService, ICurrentUserService
│   ├── Mappings/          # Mapperly mapper implementation
│   ├── Services/          # LeaveRequestService, AuthService
│   └── Validators/        # CreateLeaveRequestValidator, PaginationParamValidator
│
├── LeaveManagement.Infrastructure/
│   ├── Migrations/        # EF Core migrations
│   ├── Persistence/       # AppDbContext, DataSeeder
│   ├── Repositories/      # LeaveRequestRepository
│   └── Services/          # TokenService
│
├── LeaveManagement.API/
│   ├── Configuration/     # ConfigureSwaggerOptions
│   ├── Controllers/
│   │   ├── V1/            # LeaveRequestController (versioned)
│   │   └── AuthController.cs
│   ├── Middleware/        # ExceptionHandlingMiddleware
│   ├── Models/            # PagedResponse<T>, ApiErrorResponse
│   ├── Services/          # CurrentUserService
│   └── Program.cs
│
└── LeaveManagement.Tests/
    ├── Domain/            # LeaveRequestTests
    └── Application/       # LeaveRequestServiceTest, CreateLeaveRequestValidatorTests
```

---

## Key Design Decisions

**Rich Domain Model** — Business rules (`Approve`, `Reject`, `Cancel`) live on the `LeaveRequest` entity. The service layer orchestrates; the domain enforces rules.

**Optimistic Concurrency** — SQL Server `rowversion` column prevents two managers from approving the same request simultaneously. Returns `409 Conflict` to the losing request.

**Authorization in Service, not Controller** — `GetByIdAsync` returns `null` (404) for unauthorized access to hide record existence. `GetByEmployeeIdAsync` throws `ForbiddenAccessException` (403) because the employee ID is explicit in the URL — no existence leakage risk.

**`NoOfDays` is computed, not stored** — Derived at runtime from `StartDate` and `EndDate`. `entity.Ignore(e => e.NoOfDays)` in EF Core configuration prevents it from being mapped to a column.

**Validation is explicit, not pipeline-coupled** — FluentValidation is called manually inside services, not via MVC's `[ApiController]` auto-validation. This keeps the Application layer testable and framework-independent.

**`ICurrentUserService` defined in Application, implemented in API** — Keeps ASP.NET Core out of the Application layer while still allowing services to access the current user.

**`ConcurrencyException` wraps `DbUpdateConcurrencyException`** — The repository catches the EF Core exception and re-throws an Application-layer exception. The API layer has no dependency on EF Core for exception handling.

---

## CI/CD

GitHub Actions pipeline (`.github/workflows/ci.yml`) triggers on every push to `main` as a two-job pipeline:

**`build` job:**
1. Build the solution (`dotnet build`)
2. Run all tests (`dotnet test`)
3. Login to GitHub Container Registry (GHCR)
4. Build and push Docker image with two tags: `:latest` and `:{commit-sha}`

**`deploy` job** (runs only if `build` passes, gated via `needs: build`):
1. Authenticate to Azure via **OIDC federated credentials** — GitHub mints a short-lived token per run; no client secret is stored in GitHub Secrets
2. Deploy the SHA-tagged image to **Azure App Service** via `azure/webapps-deploy@v3`

The SHA tag (not `:latest`) is used for deployment so the running container is traceable to an exact commit. Rollback is a single `az webapp config container set` command pointing to a previous SHA tag.

---

## Cloud Deployment

The application was fully deployed to Azure to validate the production architecture end-to-end:

| Component | Service |
|---|---|
| Container Hosting | Azure App Service (Basic B1, containerized) |
| Database | Azure SQL Database (free tier) |
| Secrets | Azure Key Vault (Managed Identity — no stored credentials) |
| Observability | Application Insights (SDK auto-collection + Serilog sink) |
| Registry | GitHub Container Registry (GHCR) |

The App Service authenticates to Key Vault via **Managed Identity** — no connection string or JWT secret is stored in App Settings. CI/CD authenticates to Azure via **OIDC federated credentials** — no client secret exists anywhere in the pipeline.

> Resources were torn down after end-to-end validation to stay within the $15 budget cap. End-to-end screenshots (Swagger on `*.azurewebsites.net`, App Insights dashboard, successful CI/CD pipeline runs) are in [`/Docs/Screenshots/`](./Docs/Screenshots/).

---

## Health Checks

| Endpoint | Purpose | Failure behavior |
|---|---|---|
| `/health/live` | Is the process running? | Container restart |
| `/health/ready` | Can the app reach the database? | Remove from load balancer pool |

Liveness uses `Predicate = _ => false` — it runs no checks by design. A live app that can't reach the DB should not be restarted; it should be removed from rotation until the DB recovers.