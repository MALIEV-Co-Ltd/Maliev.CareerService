# Maliev Career Service API

Comprehensive RESTful API for managing job postings, applications, training programs, and employee development planning.

## Overview

The Career Service API is part of the Maliev Co. Ltd. microservices architecture, providing complete career management functionality including:

- **Job Posting Management** - Create, update, and publish job postings with Markdown formatting
- **Application Processing** - Track applications with status workflows and email notifications
- **Training & Learning** - Manage training programs, enrollments, and e-learning resources
- **Development Planning** - Individual Development Plans (IDPs) with career goals tracking
- **Reporting & Analytics** - HR analytics and Prometheus metrics

## Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                     Career Service API                       │
│                        (.NET 9.0)                            │
├─────────────────────────────────────────────────────────────┤
│  Controllers  │  Job Postings, Applications, Training,      │
│               │  Enrollments, Reports, IDPs, Goals          │
├───────────────┼─────────────────────────────────────────────┤
│  Services     │  Business Logic & External Service Clients  │
├───────────────┼─────────────────────────────────────────────┤
│  Data Layer   │  EF Core 9.0 + PostgreSQL                   │
├───────────────┼─────────────────────────────────────────────┤
│  Middleware   │  Logging, Exception Handling, Rate Limiting │
│               │  Concurrent Users Tracking, HTTP Metrics    │
└─────────────────────────────────────────────────────────────┘
         │                    │                    │
         ▼                    ▼                    ▼
   PostgreSQL          External Services     Prometheus
   (Primary DB)        (Employee, Upload,      Metrics
                        Email, Country)
```

## Technologies

- **.NET 9.0** - Framework
- **ASP.NET Core 9.0** - Web API
- **Entity Framework Core 9.0.9** - ORM
- **PostgreSQL** - Database (Npgsql 9.0.2)
- **FluentValidation 11.5.1** - Request validation
- **AutoMapper 12.0.1** - Object mapping
- **Serilog 8.0.2** - Structured logging
- **prometheus-net 8.2.1** - Metrics collection
- **xUnit** + **FluentAssertions** - Testing
- **Scalar** - Interactive API documentation

## Quick Start

### Prerequisites

```bash
# Required
- .NET 9 SDK
- Docker & Docker Compose
- kubectl (for K8s deployments)

# Optional
- Visual Studio 2022 or Rider
```

### Local Development

```bash
# 1. Clone repository
git clone https://github.com/MALIEV-Co-Ltd/Maliev.CareerService.git
cd Maliev.CareerService

# 2. Build solution
dotnet build Maliev.CareerService.sln

# 3. Run tests
dotnet test Maliev.CareerService.sln --verbosity normal

# 4. Start API (development mode)
cd Maliev.CareerService.Api
dotnet run

# API available at: https://localhost:8443/careers/
# Scalar UI: https://localhost:8443/scalar/v1
```

### Docker Deployment

```bash
# Build Docker image
docker build -t career-service:latest .

# Run with Docker Compose
docker-compose up -d
```

## API Endpoints

### Base URL
```
Production:  https://api.maliev.com/careers/v1
Development: http://localhost:8080/careers/v1
```

### Job Postings

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/job-postings` | Public | List active job postings (paginated) |
| GET | `/job-postings/{id}` | Public | Get specific job posting with Markdown HTML |
| POST | `/job-postings` | HRStaff | Create new job posting |
| PUT | `/job-postings/{id}` | HRStaff | Update job posting (with row version) |
| DELETE | `/job-postings/{id}` | HRStaff | Soft delete job posting |
| PATCH | `/job-postings/{id}/publish` | HRStaff | Publish job posting |

### Applications

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/applications` | HRStaff | List all applications (filtered) |
| GET | `/applications/applicant/{email}` | Public | Get applicant's applications |
| POST | `/applications` | Public | Submit job application |
| PATCH | `/applications/{id}/status` | HRStaff | Update application status |

### Training Programs

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/training-programs` | Employee | List training programs (filtered) |
| GET | `/training-programs/{id}` | Employee | Get specific program |
| POST | `/training-programs` | HRStaff | Create training program |
| PUT | `/training-programs/{id}` | HRStaff | Update training program |

### Training Enrollments

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/training-enrollments` | Employee | List own enrollments |
| POST | `/training-enrollments` | Employee | Enroll in training |
| PATCH | `/training-enrollments/{id}/complete` | HRStaff | Mark training completed |

### E-Learning Resources

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/elearning-resources` | Employee | List e-learning resources (filtered) |
| GET | `/elearning-resources/{id}` | Employee | Get specific resource |

### Reports

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/reports/application-status` | HRStaff | Application status summary |
| GET | `/reports/training-completion` | HRStaff | Training completion rates |

### Individual Development Plans (IDPs)

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/idps` | Employee | List own IDPs |
| GET | `/idps/{id}` | Employee/HRStaff | Get specific IDP |
| POST | `/idps` | Employee | Create IDP for year |
| PUT | `/idps/{id}` | Employee | Update IDP (Draft only) |
| PATCH | `/idps/{id}/submit` | Employee | Submit IDP for approval |
| PATCH | `/idps/{id}/approve` | HRStaff | Approve submitted IDP |

### Development Goals

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | `/idps/{idpId}/goals` | Employee | Add goal to IDP |
| PUT | `/goals/{id}` | Employee | Update goal (if IDP not approved) |
| PATCH | `/goals/{id}/status` | Employee | Update goal status/progress |

### Health & Metrics

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/careers/liveness` | None | Basic health check |
| GET | `/careers/readiness` | None | Database health check |
| GET | `/careers/metrics` | None | Prometheus metrics |

## Authentication

The API uses **JWT Bearer Token** authentication with role-based authorization.

### Roles

- **Employee** - Submit applications, enroll in training, manage own IDPs
- **HRStaff** - Manage job postings, review applications, approve IDPs

### Usage

```bash
# Include JWT token in Authorization header
curl -H "Authorization: Bearer <your-jwt-token>" \
     https://api.maliev.com/careers/v1/idps
```

### JWT Token Claims

```json
{
  "sub": "employee-uuid",
  "email": "user@maliev.com",
  "role": "Employee",
  "exp": 1640995200
}
```

## Rate Limiting

The API implements fixed-window rate limiting:

| User Type | Limit | Window |
|-----------|-------|--------|
| Anonymous | 100 requests | 1 minute |
| Authenticated (Employee) | 200 requests | 1 minute |
| Admin (HRStaff) | 500 requests | 1 minute |

**429 Response** when limit exceeded:
```json
{
  "error": "Rate limit exceeded. Please try again later."
}
```

## Error Response Format

All errors return consistent JSON structure:

### 400 Bad Request
```json
{
  "error": "Validation failed for field 'email': must be valid email format"
}
```

### 401 Unauthorized
```json
{
  "error": "User ID not found in claims"
}
```

### 403 Forbidden
```json
{
  "error": "You can only view your own IDPs"
}
```

### 404 Not Found
```json
{
  "error": "Job posting abc123 not found"
}
```

### 409 Conflict
```json
{
  "error": "An IDP for year 2025 already exists for this employee"
}
```

### 500 Internal Server Error
```json
{
  "error": "An unexpected error occurred. Please contact support."
}
```

## API Usage Examples

### Submit Job Application

```bash
# Public endpoint - no authentication required
curl -X POST https://api.maliev.com/careers/v1/applications \
  -H "Content-Type: application/json" \
  -d '{
    "jobPostingId": "a1b2c3d4-5678-90ab-cdef-1234567890ab",
    "applicantFirstName": "John",
    "applicantLastName": "Doe",
    "applicantEmail": "john.doe@example.com",
    "applicantPhone": "+1234567890",
    "applicantCountryCode": "US",
    "resumeFileId": "f1e2d3c4-5678-90ab-cdef-1234567890cd",
    "coverLetter": "I am excited to apply for this position...",
    "additionalFileIds": []
  }'

# Response: 201 Created
{
  "id": "b2c3d4e5-6789-01bc-def0-234567890abc",
  "jobPostingId": "a1b2c3d4-5678-90ab-cdef-1234567890ab",
  "applicantEmail": "john.doe@example.com",
  "status": "Submitted",
  "appliedAt": "2025-01-15T10:30:00Z"
}
```

### List Job Postings (Public)

```bash
# Public endpoint with filtering and pagination
curl "https://api.maliev.com/careers/v1/job-postings?department=Engineering&location=Remote&limit=10&offset=0"

# Response: 200 OK
{
  "items": [
    {
      "id": "a1b2c3d4-5678-90ab-cdef-1234567890ab",
      "positionTitle": "Senior Software Engineer",
      "positionCode": "ENG-2025-001",
      "department": "Engineering",
      "location": "Remote",
      "employmentType": "Full-Time",
      "salaryMin": 80000.00,
      "salaryMax": 120000.00,
      "currency": "USD",
      "descriptionHtml": "<h2>About the Role</h2><p>We are seeking...</p>",
      "requirementsHtml": "<ul><li>5+ years experience...</li></ul>",
      "isActive": true,
      "publishedAt": "2025-01-10T08:00:00Z",
      "applicationDeadline": "2025-02-10T23:59:59Z"
    }
  ],
  "page": 1,
  "pageSize": 10,
  "totalCount": 5,
  "totalPages": 1
}
```

### Enroll in Training (Authenticated)

```bash
# Employee authentication required
curl -X POST https://api.maliev.com/careers/v1/training-enrollments \
  -H "Authorization: Bearer eyJhbGc..." \
  -H "Content-Type: application/json" \
  -d '{
    "trainingProgramId": "c3d4e5f6-7890-12cd-ef01-345678901bcd"
  }'

# Response: 201 Created
{
  "id": "d4e5f6g7-8901-23de-f012-456789012cde",
  "trainingProgramId": "c3d4e5f6-7890-12cd-ef01-345678901bcd",
  "employeeId": "e5f6g7h8-9012-34ef-0123-567890123def",
  "enrollmentType": "Voluntary",
  "status": "Enrolled",
  "enrolledAt": "2025-01-15T14:20:00Z"
}
```

### Create Job Posting (HRStaff)

```bash
# HRStaff authentication required
curl -X POST https://api.maliev.com/careers/v1/job-postings \
  -H "Authorization: Bearer eyJhbGc..." \
  -H "Content-Type: application/json" \
  -d '{
    "positionTitle": "DevOps Engineer",
    "positionCode": "OPS-2025-002",
    "department": "Operations",
    "location": "Hybrid",
    "employmentType": "Full-Time",
    "salaryMin": 70000.00,
    "salaryMax": 100000.00,
    "currency": "USD",
    "description": "# About the Role\n\nWe are looking for...",
    "requirements": "## Required Skills\n\n- Kubernetes\n- Docker\n- CI/CD",
    "responsibilities": "## Key Responsibilities\n\n- Manage infrastructure\n- Deploy applications",
    "applicationDeadline": "2025-03-01T23:59:59Z"
  }'

# Response: 201 Created
{
  "id": "f6g7h8i9-0123-45ef-1234-678901234efg",
  "positionTitle": "DevOps Engineer",
  "positionCode": "OPS-2025-002",
  "isActive": false,
  "publishedAt": null,
  "rowVersion": "AAAAAAAAB9E="
}
```

### Update Application Status (HRStaff)

```bash
# HRStaff authentication required - includes concurrency control
curl -X PATCH https://api.maliev.com/careers/v1/applications/b2c3d4e5-6789-01bc-def0-234567890abc/status \
  -H "Authorization: Bearer eyJhbGc..." \
  -H "Content-Type: application/json" \
  -d '{
    "newStatus": "UnderReview",
    "reason": "Application meets initial qualifications",
    "rowVersion": "AAAAAAAAB8Q="
  }'

# Response: 200 OK
{
  "id": "b2c3d4e5-6789-01bc-def0-234567890abc",
  "status": "UnderReview",
  "updatedAt": "2025-01-16T09:15:00Z"
}
```

### Create and Submit IDP (Employee)

```bash
# Step 1: Create IDP (Employee)
curl -X POST https://api.maliev.com/careers/v1/idps \
  -H "Authorization: Bearer eyJhbGc..." \
  -H "Content-Type: application/json" \
  -d '{
    "planYear": 2025
  }'

# Response: 201 Created
{
  "id": "g7h8i9j0-1234-56ef-2345-789012345fgh",
  "employeeId": "e5f6g7h8-9012-34ef-0123-567890123def",
  "planYear": 2025,
  "status": "Draft",
  "goals": []
}

# Step 2: Add Goals to IDP
curl -X POST https://api.maliev.com/careers/v1/idps/g7h8i9j0-1234-56ef-2345-789012345fgh/goals \
  -H "Authorization: Bearer eyJhbGc..." \
  -H "Content-Type: application/json" \
  -d '{
    "goalTitle": "Complete Cloud Architecture Certification",
    "description": "Obtain AWS Solutions Architect Professional certification",
    "targetCompletionDate": "2025-06-30T00:00:00Z",
    "category": "Technical"
  }'

# Step 3: Submit IDP for Approval
curl -X PATCH https://api.maliev.com/careers/v1/idps/g7h8i9j0-1234-56ef-2345-789012345fgh/submit \
  -H "Authorization: Bearer eyJhbGc..."

# Response: 200 OK
{
  "id": "g7h8i9j0-1234-56ef-2345-789012345fgh",
  "status": "Submitted",
  "submittedAt": "2025-01-16T11:00:00Z"
}
```

## External Service Dependencies

The Career Service integrates with these external services:

### Employee Service
- **Purpose**: Employee validation, profile retrieval
- **Endpoint**: `${EmployeeService:BaseUrl}/v1/employees/{id}`
- **Fallback**: Returns validation failure if unavailable

### Upload Service
- **Purpose**: Resume and document file management
- **Endpoint**: `${UploadService:BaseUrl}/v1/files/{id}`
- **Fallback**: File validation fails gracefully

### Email Service
- **Purpose**: Application confirmations, status change notifications
- **Endpoint**: `${EmailService:BaseUrl}/v1/emails/send`
- **Fallback**: Email failures logged but don't block operations

### Country Service
- **Purpose**: Country code validation and name resolution
- **Endpoint**: `${CountryService:BaseUrl}/v1/countries/{code}`
- **Fallback**: Returns country code if service unavailable

## Configuration

### Required Environment Variables

```bash
# Database
CareerDbContext="Server=localhost;Port=5432;Database=career_db;User Id=postgres;Password=<password>;"

# JWT Authentication
Jwt__Issuer="https://auth.maliev.com"
Jwt__Audience="https://api.maliev.com"
Jwt__SecretKey="<your-secret-key>"

# External Services
ExternalServices__EmployeeService__BaseUrl="https://api.maliev.com/employees"
ExternalServices__UploadService__BaseUrl="https://api.maliev.com/uploads"
ExternalServices__EmailService__BaseUrl="https://api.maliev.com/emails"
ExternalServices__CountryService__BaseUrl="https://api.maliev.com/countries"
```

### Google Secret Manager (Production)

Secrets are mounted at `/mnt/secrets/` in production:

```bash
/mnt/secrets/
├── CareerDbContext
├── Jwt__SecretKey
├── ExternalServices__EmployeeService__ApiKey
└── ExternalServices__EmailService__ApiKey
```

## Prometheus Metrics

Available at `/careers/metrics`:

### Custom Metrics

- `career_job_applications_total` - Counter with `status` label
- `career_training_enrollments_total` - Counter with `status` label
- `career_active_job_postings` - Gauge of active postings
- `career_concurrent_users` - Gauge of concurrent API users

### HTTP Metrics (automatic)

- `http_requests_received_total` - Total HTTP requests
- `http_requests_in_progress` - Concurrent requests
- `http_request_duration_seconds` - Response time histogram

## Database Migrations

```bash
# Create new migration
dotnet ef migrations add MigrationName \
  --project Maliev.CareerService.Data \
  --startup-project Maliev.CareerService.Api

# Apply migrations (development)
export CareerDbContext="Server=localhost;Port=5432;Database=career_db;User Id=postgres;Password=postgres;"
dotnet ef database update \
  --project Maliev.CareerService.Data \
  --startup-project Maliev.CareerService.Api

# Apply migrations (production via port-forward)
kubectl port-forward -n maliev-prod postgres-cluster-1 5432:5432 &
export CareerDbContext="Server=localhost;Port=5432;Database=career_db;User Id=postgres;Password=<prod-password>;"
dotnet ef database update --project Maliev.CareerService.Data --startup-project Maliev.CareerService.Api
```

## Testing

```bash
# Run all tests
dotnet test Maliev.CareerService.sln --verbosity normal

# Run specific test category
dotnet test --filter "FullyQualifiedName~Integration"
dotnet test --filter "FullyQualifiedName~Contract"

# Generate coverage report
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura
```

## GitOps Deployment

The service uses ArgoCD for GitOps deployments:

```bash
# Repository: maliev-gitops
# Path: 3-apps/career-service/

overlays/
├── development/    # maliev-dev namespace
├── staging/        # maliev-staging namespace
└── production/     # maliev-prod namespace
```

### Manual Deployment (if needed)

```bash
# Development
kubectl apply -k 3-apps/career-service/overlays/development

# Production
kubectl apply -k 3-apps/career-service/overlays/production
```

## Troubleshooting Guide

### API Returns 500 Internal Server Error

**Symptoms**: All requests return 500 errors, logs show database connection failures

**Diagnosis**:
```bash
# Check database connection
kubectl logs -n maliev-dev deployment/career-service --tail=50 | grep "database"

# Check PostgreSQL status
kubectl get pods -n maliev-dev -l app=postgres
```

**Solutions**:
1. **Database not available**: Ensure PostgreSQL is running and accessible
   ```bash
   kubectl get pods -n maliev-dev | grep postgres
   ```

2. **Connection string incorrect**: Verify secrets are correctly mounted
   ```bash
   kubectl exec -n maliev-dev deployment/career-service -- ls /mnt/secrets
   ```

3. **Migration not applied**: Apply pending migrations
   ```bash
   kubectl port-forward -n maliev-dev postgres-cluster-1 5432:5432
   dotnet ef database update --project Maliev.CareerService.Data
   ```

### Authentication Fails (401 Unauthorized)

**Symptoms**: Requests with valid JWT tokens return 401

**Diagnosis**:
```bash
# Check JWT configuration in logs
kubectl logs -n maliev-dev deployment/career-service | grep "JWT"
```

**Solutions**:
1. **JWT secret not configured**: Verify `Jwt__SecretKey` is in secrets
   ```bash
   kubectl get secret maliev-service-secrets -n maliev-dev -o yaml
   ```

2. **Token expired**: Check token expiration claim (`exp`)
   ```bash
   # Decode JWT token
   echo "eyJhbGc..." | base64 -d
   ```

3. **Issuer/Audience mismatch**: Ensure token `iss` and `aud` match configuration

### Rate Limiting Issues (429 Too Many Requests)

**Symptoms**: Requests consistently return 429 after reaching limit

**Diagnosis**:
```bash
# Check rate limit configuration in logs
kubectl logs -n maliev-dev deployment/career-service | grep "Rate limit"
```

**Solutions**:
1. **Anonymous user hitting limit**: Authenticate to get higher limits (200 req/min vs 100)
2. **Legitimate high traffic**: Request HRStaff role for 500 req/min limit
3. **Adjust rate limits**: Modify `Program.cs` rate limiter configuration if business requirements change

### External Service Calls Failing

**Symptoms**: Operations involving employee validation, file uploads, or emails fail

**Diagnosis**:
```bash
# Check external service logs
kubectl logs -n maliev-dev deployment/career-service | grep "External service"

# Test external service connectivity
kubectl exec -n maliev-dev deployment/career-service -- curl http://employee-service:8080/employees/v1/health
```

**Solutions**:
1. **Service unavailable**: Check if dependent services are running
   ```bash
   kubectl get pods -n maliev-dev | grep -E "employee|upload|email|country"
   ```

2. **Wrong service URLs**: Verify `ExternalServices__*__BaseUrl` configuration
   ```bash
   kubectl get configmap career-service-config -n maliev-dev -o yaml
   ```

3. **Network policy blocking**: Check Kubernetes network policies
   ```bash
   kubectl get networkpolicies -n maliev-dev
   ```

### Docker Build Fails

**Symptoms**: `docker build` command fails with errors

**Diagnosis**:
```bash
# Check Docker build output
docker build -t career-service:latest . 2>&1 | tee build.log
```

**Solutions**:
1. **Dockerfile not found**: Ensure you're in project root
   ```bash
   ls -la | grep Dockerfile
   ```

2. **.NET SDK version mismatch**: Update Dockerfile base image
   ```dockerfile
   FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
   ```

3. **Build context too large**: Add `.dockerignore` file
   ```
   **/bin/
   **/obj/
   **/.vs/
   **/node_modules/
   ```

### Tests Fail with Testcontainers Timeout

**Symptoms**: Integration tests fail with "The operation has timed out" for PostgreSQL container

**Diagnosis**:
```bash
# Check Docker is running
docker ps
```

**Solutions**:
1. **Docker not running**: Start Docker Desktop
   ```bash
   # Windows
   "C:\Program Files\Docker\Docker\Docker Desktop.exe"

   # Linux/Mac
   sudo systemctl start docker
   ```

2. **Insufficient Docker resources**: Increase Docker memory/CPU limits in Docker Desktop settings

3. **Network issues**: Check Docker network connectivity
   ```bash
   docker network ls
   docker network inspect bridge
   ```

### Migration Fails with "relation already exists"

**Symptoms**: `dotnet ef database update` fails with duplicate table errors

**Diagnosis**:
```bash
# Check applied migrations
dotnet ef migrations list --project Maliev.CareerService.Data
```

**Solutions**:
1. **Migrations table out of sync**: Manually fix `__EFMigrationsHistory` table
   ```sql
   SELECT * FROM "__EFMigrationsHistory" ORDER BY "MigrationId";
   ```

2. **Partial migration applied**: Drop incomplete tables and re-run
   ```sql
   DROP TABLE IF EXISTS problem_table;
   ```

3. **Start fresh** (development only):
   ```bash
   dotnet ef database drop --project Maliev.CareerService.Data --force
   dotnet ef database update --project Maliev.CareerService.Data
   ```

### Prometheus Metrics Not Showing

**Symptoms**: `/careers/metrics` endpoint returns 404 or empty

**Diagnosis**:
```bash
# Test metrics endpoint
curl http://localhost:8080/careers/metrics
```

**Solutions**:
1. **Endpoint not mapped**: Verify `Program.cs` has `app.MapMetrics("/careers/metrics")`

2. **Middleware order incorrect**: Ensure `UseHttpMetrics()` is before `UseEndpoints()`

3. **ServiceMonitor not configured**: Check Prometheus ServiceMonitor in Kubernetes
   ```bash
   kubectl get servicemonitor -n maliev-dev
   ```

### Performance Issues / Slow Queries

**Symptoms**: API responses are slow (>1s for simple queries)

**Diagnosis**:
```bash
# Check database query performance
kubectl logs -n maliev-dev deployment/career-service | grep "Executed DbCommand"
```

**Solutions**:
1. **Missing indexes**: Review `JobPostingConfiguration.cs` composite index on `(IsActive, PublishedAt, ApplicationDeadline)`

2. **N+1 query problem**: Add `.Include()` for navigation properties
   ```csharp
   _dbContext.EmployeeTrainingEnrollments
       .Include(e => e.TrainingProgram)  // Avoid N+1
       .Where(e => e.EmployeeId == employeeId)
   ```

3. **Large result sets**: Implement pagination on all list endpoints

4. **Enable query logging** (development only):
   ```json
   {
     "Logging": {
       "LogLevel": {
         "Microsoft.EntityFrameworkCore.Database.Command": "Information"
       }
     }
   }
   ```

## Support & Documentation

- **API Documentation**: [Scalar UI](https://api.maliev.com/careers/scalar/v1)
- **Issues**: [GitHub Issues](https://github.com/MALIEV-Co-Ltd/Maliev.CareerService/issues)

## License

Proprietary - Maliev Co. Ltd. © 2025
