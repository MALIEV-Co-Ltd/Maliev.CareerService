# Quickstart Guide: Career Service Web API

**Feature**: [spec.md](spec.md)
**Created**: 2025-10-21
**Status**: Draft - Phase 1 Design

This guide provides step-by-step instructions for setting up a local development environment and running the Career Service Web API.

---

## Prerequisites

Before you begin, ensure you have the following installed:

| Tool | Version | Purpose |
|------|---------|---------|
| .NET SDK | 9.0 or later | C# development and compilation |
| Docker Desktop | Latest | PostgreSQL container for local database |
| Git | Latest | Version control |
| VS Code or Rider | Latest | Code editor/IDE (recommended) |
| kubectl | Latest | Kubernetes CLI (for production debugging) |

### Verify Installation

```powershell
# Verify .NET SDK
dotnet --version  # Should show 9.x.x

# Verify Docker
docker --version

# Verify kubectl (optional)
kubectl version --client
```

---

## Project Structure

```
Maliev.CareerService/
├── Maliev.CareerService.Api/          # Web API project
│   ├── Controllers/                   # API controllers
│   ├── Services/                      # Business logic services
│   ├── Validators/                    # FluentValidation validators
│   ├── DTOs/                          # Data transfer objects
│   ├── Middleware/                    # Custom middleware
│   ├── appsettings.json               # Configuration (no secrets!)
│   └── Program.cs                     # Application entry point
│
├── Maliev.CareerService.Data/         # Data access layer
│   ├── Entities/                      # EF Core entity classes
│   ├── Configurations/                # Fluent API configurations
│   ├── Migrations/                    # EF Core migrations
│   └── CareerDbContext.cs             # Database context
│
├── Maliev.CareerService.Tests/        # Test project
│   ├── Integration/                   # Integration tests (PostgreSQL)
│   ├── Unit/                          # Unit tests
│   ├── Fixtures/                      # Test fixtures (TestDatabaseFixture)
│   └── docker-compose.test.yml        # PostgreSQL container for tests
│
└── Maliev.CareerService.sln           # Solution file
```

---

## Step 1: Clone the Repository

```powershell
# Clone the repository
git clone https://github.com/MALIEV-Co-Ltd/maliev-services.git
cd maliev-services/Maliev.CareerService

# Create feature branch (if working on new feature)
git checkout -b feature/your-feature-name
```

---

## Step 2: Start PostgreSQL Database

The Career Service requires a PostgreSQL 18 database. Use Docker to run a local instance:

```powershell
# Start PostgreSQL container
docker run --name career-db `
  -e POSTGRES_PASSWORD=localDevPassword123 `
  -e POSTGRES_USER=postgres `
  -e POSTGRES_DB=career_service_dev `
  -p 5432:5432 `
  -d postgres:18

# Verify container is running
docker ps | Select-String "career-db"

# View PostgreSQL logs (optional)
docker logs -f career-db
```

### Stop/Start PostgreSQL Container

```powershell
# Stop the container
docker stop career-db

# Start existing container
docker start career-db

# Remove container (when done)
docker stop career-db
docker rm career-db
```

---

## Step 3: Configure Connection String

Create a `appsettings.Development.json` file in `Maliev.CareerService.Api/`:

```json
{
  "ConnectionStrings": {
    "CareerDbContext": "Server=localhost;Port=5432;Database=career_service_dev;User Id=postgres;Password=localDevPassword123;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Information"
    }
  },
  "JwtSettings": {
    "Issuer": "https://auth.maliev.local",
    "Audience": "career-service",
    "PublicKey": "DEV_PUBLIC_KEY_PLACEHOLDER"
  },
  "RateLimiting": {
    "Anonymous": {
      "PermitLimit": 100,
      "WindowSeconds": 60
    },
    "Applicant": {
      "PermitLimit": 200,
      "WindowSeconds": 60
    },
    "Employee": {
      "PermitLimit": 300,
      "WindowSeconds": 60
    },
    "HRStaff": {
      "PermitLimit": 500,
      "WindowSeconds": 60
    }
  },
  "ExternalServices": {
    "EmployeeService": {
      "BaseUrl": "http://localhost:8081/employee/api/v1",
      "Timeout": 5000
    },
    "UploadService": {
      "BaseUrl": "http://localhost:8082/upload/api/v1",
      "Timeout": 10000
    },
    "AuthService": {
      "BaseUrl": "http://localhost:8083/auth/api/v1",
      "Timeout": 3000
    },
    "CountryService": {
      "BaseUrl": "http://localhost:8084/country/api/v1",
      "Timeout": 3000
    }
  }
}
```

**IMPORTANT**: Never commit `appsettings.Development.json` to version control. Add it to `.gitignore`:

```gitignore
# Local development configuration
appsettings.Development.json
```

---

## Step 4: Apply Database Migrations

Run EF Core migrations to create the database schema:

```powershell
# Navigate to Data project directory
cd Maliev.CareerService.Data

# Install EF Core CLI (if not already installed)
dotnet tool install --global dotnet-ef

# Apply migrations
dotnet ef database update --startup-project ../Maliev.CareerService.Api

# Verify migration applied
dotnet ef migrations list --startup-project ../Maliev.CareerService.Api
```

### Create New Migration (when modifying entities)

```powershell
# Create migration
dotnet ef migrations add YourMigrationName --startup-project ../Maliev.CareerService.Api

# Review generated migration file in Migrations/

# Apply migration
dotnet ef database update --startup-project ../Maliev.CareerService.Api
```

---

## Step 5: Build the Solution

```powershell
# Navigate to solution root
cd ..

# Restore NuGet packages
dotnet restore Maliev.CareerService.sln

# Build solution (treat warnings as errors)
dotnet build Maliev.CareerService.sln --configuration Debug

# Expected output: Build succeeded. 0 Warning(s)
```

---

## Step 6: Run the API

```powershell
# Run the API project
cd Maliev.CareerService.Api
dotnet run

# API will start on:
# - HTTPS: https://localhost:5001/career/api/v1
# - HTTP: http://localhost:5000/career/api/v1
```

### Verify API is Running

Open your browser and navigate to:
- **Swagger UI**: http://localhost:5000/career/swagger
- **Scalar UI**: http://localhost:5000/career/scalar/v1
- **Health Check (Liveness)**: http://localhost:5000/career/liveness
- **Health Check (Readiness)**: http://localhost:5000/career/readiness
- **Metrics**: http://localhost:5000/career/metrics

---

## Step 7: Run Tests

The Career Service uses **PostgreSQL-only testing** (no in-memory database). Tests use Testcontainers to spin up temporary PostgreSQL instances.

### Prerequisites for Testing

Ensure Docker Desktop is running (Testcontainers requires Docker).

### Run All Tests

```powershell
# Navigate to solution root
cd R:\maliev\Maliev.CareerService

# Run all tests
dotnet test Maliev.CareerService.sln --verbosity normal

# Run tests with detailed output
dotnet test Maliev.CareerService.sln --verbosity detailed --logger "console;verbosity=detailed"

# Run tests with coverage (optional)
dotnet test Maliev.CareerService.sln --collect:"XPlat Code Coverage"
```

### Run Specific Test Categories

```powershell
# Run only unit tests
dotnet test --filter "Category=Unit"

# Run only integration tests
dotnet test --filter "Category=Integration"

# Run specific test class
dotnet test --filter "FullyQualifiedName~JobPostingControllerTests"
```

### Troubleshooting Tests

If tests fail with "Docker is not running":
1. Start Docker Desktop
2. Wait for Docker to fully initialize
3. Re-run tests

---

## Step 8: Make API Requests (Manual Testing)

### Using curl

```powershell
# List active job postings (public endpoint)
curl http://localhost:5000/career/api/v1/job-postings

# Get specific job posting
curl http://localhost:5000/career/api/v1/job-postings/{guid}

# Submit job application (public endpoint)
curl -X POST http://localhost:5000/career/api/v1/job-applications `
  -H "Content-Type: application/json" `
  -d '{
    "job_posting_id": "{guid}",
    "applicant_first_name": "John",
    "applicant_last_name": "Doe",
    "applicant_email": "john.doe@example.com",
    "applicant_phone": "+66812345678",
    "applicant_country_code": "TH",
    "resume_file_id": "{guid}",
    "cover_letter": "I am interested in this position..."
  }'
```

### Using Swagger UI

1. Navigate to http://localhost:5000/career/swagger
2. Expand the endpoint you want to test
3. Click "Try it out"
4. Fill in request parameters
5. Click "Execute"
6. View response

### Authenticated Endpoints

For endpoints requiring authentication, you need a JWT token from the Auth Service:

```powershell
# Get JWT token (replace with actual Auth Service call)
$token = "your-jwt-token-here"

# Make authenticated request
curl -X GET http://localhost:5000/career/api/v1/training-programs `
  -H "Authorization: Bearer $token"
```

---

## Common Development Tasks

### View Database Records

```powershell
# Connect to PostgreSQL container
docker exec -it career-db psql -U postgres -d career_service_dev

# List all tables
\dt

# View job postings
SELECT id, position_title, department, is_active FROM job_postings;

# View applications
SELECT id, applicant_email, status FROM job_applications;

# Exit psql
\q
```

### Reset Database (Clean Slate)

```powershell
# Drop and recreate database
docker exec -it career-db psql -U postgres -c "DROP DATABASE career_service_dev;"
docker exec -it career-db psql -U postgres -c "CREATE DATABASE career_service_dev;"

# Reapply migrations
cd Maliev.CareerService.Data
dotnet ef database update --startup-project ../Maliev.CareerService.Api
```

### Watch Mode (Auto-Reload on Code Changes)

```powershell
# Run API with hot reload
cd Maliev.CareerService.Api
dotnet watch run

# API will automatically restart when you save file changes
```

### View Application Logs

Logs are written to stdout in JSON format (Serilog). To view structured logs:

```powershell
# Run API and pipe logs to file
dotnet run > logs.json

# View logs in real-time with filtering (PowerShell)
Get-Content logs.json -Tail 50 -Wait | Select-String "Error"
```

---

## Local Development with External Services

The Career Service depends on 4 external services:
1. Employee Service (employee data lookup)
2. Upload Service (file upload/download)
3. Auth Service (JWT validation)
4. Country Service (country name lookup)

### Option 1: Mock External Services (Recommended for Local Dev)

Use Moq or WireMock to mock external service responses. See `Maliev.CareerService.Tests/Mocks/` for examples.

### Option 2: Run External Services Locally

```powershell
# Port forward to Kubernetes services (if available)
kubectl port-forward -n maliev-dev svc/maliev-employee-service 8081:8080 &
kubectl port-forward -n maliev-dev svc/maliev-upload-service 8082:8080 &
kubectl port-forward -n maliev-dev svc/maliev-auth-service 8083:8080 &
kubectl port-forward -n maliev-dev svc/maliev-country-service 8084:8080 &

# Update appsettings.Development.json with correct URLs (already configured above)
```

### Option 3: Use Staging Environment

Point your local API to staging external services:

```json
"ExternalServices": {
  "EmployeeService": {
    "BaseUrl": "https://staging-api.maliev.com/employee/api/v1"
  }
}
```

**WARNING**: Be careful not to corrupt staging data during development!

---

## Debugging Tips

### Debug in Visual Studio Code

1. Open `Maliev.CareerService` folder in VS Code
2. Press `F5` to start debugging
3. Set breakpoints in code
4. Use Debug Console to inspect variables

### Debug in JetBrains Rider

1. Open `Maliev.CareerService.sln` in Rider
2. Set breakpoints
3. Click "Debug" (Shift+F9)
4. Use debugger tools to step through code

### Common Issues

| Issue | Solution |
|-------|----------|
| Port 5000 already in use | Kill process using port: `Stop-Process -Id (Get-NetTCPConnection -LocalPort 5000).OwningProcess -Force` |
| PostgreSQL connection refused | Ensure Docker container is running: `docker start career-db` |
| Migration not applied | Run `dotnet ef database update` |
| Tests fail with Docker error | Start Docker Desktop and wait for initialization |
| 401 Unauthorized on protected endpoints | Provide valid JWT token in `Authorization: Bearer {token}` header |

---

## Next Steps

After successfully running the API locally:

1. **Explore API**: Use Swagger UI to test all endpoints
2. **Write Tests**: Add unit and integration tests for new features
3. **Add Features**: Implement new functionality according to [spec.md](spec.md)
4. **Review Code**: Follow code review checklist before committing
5. **Deploy**: Push to `develop` branch to trigger CI/CD pipeline

---

## Production Deployment

For production deployment instructions, see:
- [CLAUDE.md](../../CLAUDE.md) - CI/CD workflows and GitOps deployment
- `maliev-gitops` repository - Kubernetes manifests and ArgoCD configuration

---

## Useful Commands Cheat Sheet

```powershell
# Start PostgreSQL
docker start career-db

# Run API
cd Maliev.CareerService.Api && dotnet run

# Run tests
dotnet test Maliev.CareerService.sln

# Apply migrations
cd Maliev.CareerService.Data && dotnet ef database update --startup-project ../Maliev.CareerService.Api

# Create migration
dotnet ef migrations add MigrationName --startup-project ../Maliev.CareerService.Api

# View database
docker exec -it career-db psql -U postgres -d career_service_dev

# Stop PostgreSQL
docker stop career-db

# Clean build
dotnet clean && dotnet build

# Restore packages
dotnet restore
```

---

## Support and Resources

- **Project Documentation**: [spec.md](spec.md), [plan.md](plan.md), [research.md](research.md)
- **Codebase Guidelines**: [CLAUDE.md](../../CLAUDE.md)
- **Entity Framework Core Docs**: https://learn.microsoft.com/en-us/ef/core/
- **ASP.NET Core Docs**: https://learn.microsoft.com/en-us/aspnet/core/
- **PostgreSQL Docs**: https://www.postgresql.org/docs/18/

---

**Happy Coding! 🚀**
