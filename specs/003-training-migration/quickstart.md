# Developer Quickstart: Training Migration

**Feature**: Training Records and Skills Migration
**Branch**: `003-training-migration`
**Date**: 2025-12-28

## Prerequisites

- .NET 10.0 SDK
- Docker Desktop (for Testcontainers)
- IDE (Visual Studio 2022, Rider, or VS Code)
- PostgreSQL client (optional, for manual DB inspection)

## Initial Setup

### 1. Clone and Switch to Feature Branch

```bash
git fetch origin
git checkout 003-training-migration
```

### 2. Database Migration

```bash
# Create and apply migration
dotnet ef migrations add AddTrainingMigrationEntities \
    --project Maliev.CareerService.Data \
    --startup-project Maliev.CareerService.Api

dotnet ef database update \
    --project Maliev.CareerService.Data \
    --startup-project Maliev.CareerService.Api
```

**Verify migration**:
```sql
-- Connect to PostgreSQL
psql -h localhost -U postgres -d career_service

-- Check new tables
\dt training_records
\dt skills
\dt mandatory_training_requirements

-- Verify indexes
\di idx_training_records_employee
\di idx_skills_employee_name_unique
```

### 3. Build Solution

```bash
dotnet build
```

**Expected output**: Zero warnings (warnings-as-errors enabled)

## Running Locally

### Start Career Service

```bash
cd Maliev.CareerService.Api
dotnet run
```

Service will start on http://localhost:8080

### Verify Health

```bash
curl http://localhost:8080/career/health
```

Expected response:
```json
{
  "status": "Healthy",
  "results": {
    "PostgreSQL": { "status": "Healthy" },
    "RabbitMQ": { "status": "Healthy" }
  }
}
```

### Access API Documentation

Open browser: http://localhost:8080/scalar/v1

## Testing

### Run All Tests

```bash
dotnet test
```

Tests use **Testcontainers** - Docker must be running!

### Run Specific Test Categories

```bash
# Unit tests only
dotnet test --filter Category=Unit

# Integration tests only (requires Docker)
dotnet test --filter Category=Integration

# Training migration tests only
dotnet test --filter FullyQualifiedName~TrainingRecord
```

### Test with Testcontainers

Integration tests automatically:
1. Start PostgreSQL container
2. Start RabbitMQ container
3. Apply migrations
4. Run tests
5. Cleanup containers

**Example test**:
```csharp
public class TrainingRecordsControllerTests : IClassFixture<CareerServiceTestFixture>
{
    [Fact]
    public async Task RecordTrainingCompletion_ValidRequest_Returns201()
    {
        // Arrange - Testcontainers already running
        var client = _fixture.CreateClient();
        var request = new RecordTrainingCompletionRequest
        {
            CourseName = "Test Training",
            CompletionDate = DateTime.UtcNow.AddDays(-1),
            TrainingType = TrainingType.Online
        };

        // Act
        var response = await client.PostAsJsonAsync(
            $"/career/v1/employees/{employeeId}/training-records",
            request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }
}
```

## Sample API Requests

### 1. Record Training Completion

```bash
# Get JWT token first
TOKEN="your-jwt-token-here"

# Record completion
curl -X POST http://localhost:8080/career/v1/employees/{employeeId}/training-records \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "courseName": "Workplace Safety Training",
    "completionDate": "2025-01-15T00:00:00Z",
    "expirationDate": "2026-01-15T00:00:00Z",
    "trainingType": "Online",
    "provider": "SafetyFirst Inc.",
    "score": 95.5
  }'
```

### 2. Get Training Records

```bash
curl -X GET http://localhost:8080/career/v1/employees/{employeeId}/training-records \
  -H "Authorization: Bearer $TOKEN"
```

### 3. Add Employee Skill

```bash
curl -X POST http://localhost:8080/career/v1/employees/{employeeId}/skills \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "skillName": "Python",
    "proficiencyLevel": "Advanced",
    "isDevelopmentArea": false,
    "notes": "Used daily for data analysis"
  }'
```

### 4. Create Mandatory Training Requirement

```bash
curl -X POST http://localhost:8080/career/v1/mandatory-training \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "trainingProgramId": "{trainingProgramId}",
    "departmentId": null,
    "positionId": null,
    "completionDeadlineDays": 30,
    "recertificationMonths": 12
  }'
```

### 5. Get Compliance Report

```bash
curl -X GET http://localhost:8080/career/v1/reports/training-compliance \
  -H "Authorization: Bearer $TOKEN"
```

## Background Services

### Testing Locally

Background services run automatically:
- `CertificationExpirationReminderBackgroundService` - Runs daily at app startup + every 24 hours
- `OverdueTrainingEscalationBackgroundService` - Runs daily at app startup + every 24 hours

**Trigger manually** (for testing):
```csharp
// In integration test
var service = _fixture.GetService<CertificationExpirationReminderBackgroundService>();
await service.StartAsync(CancellationToken.None);
```

**View logs**:
```bash
# Tail logs
dotnet run | grep -i "CertificationExpiration\|OverdueTraining"
```

## Simulating Employee Service Events

### Publish Test Event

```csharp
// In integration test or test console app
var publishEndpoint = serviceProvider.GetRequiredService<IPublishEndpoint>();

await publishEndpoint.Publish(new EmployeeCreatedEvent(
    EmployeeId: Guid.NewGuid(),
    DepartmentId: departmentId,
    PositionId: positionId,
    StartDate: DateTime.UtcNow
));
```

### Verify Consumer Processing

Check database for new enrollments:
```sql
SELECT * FROM enrollments
WHERE employee_id = '{employeeId}'
ORDER BY created_at DESC;
```

## Authorization Testing

### Get Test JWT Token

```bash
# Using IAM Service
curl -X POST http://localhost:8080/iam/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "username": "test.user@maliev.com",
    "password": "test123"
  }'
```

### Test Permission Scenarios

1. **Employee viewing own records** - requires `career.training.view-own`
2. **Manager viewing team records** - requires `career.training.view-team`
3. **HR admin managing all records** - requires `career.training.manage` or `career.*`

**Example permission check**:
```csharp
// In controller test
var claims = new List<Claim>
{
    new Claim("sub", employeeId.ToString()),
    new Claim("permissions", "Permission:career.training.view-own")
};

var token = GenerateTestToken(claims);
client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
```

## Database Inspection

### View Training Records

```sql
SELECT
    tr.id,
    tr.employee_id,
    tr.course_name,
    tr.completion_date,
    tr.expiration_date,
    tr.status,
    tp.name AS training_program_name
FROM training_records tr
LEFT JOIN training_programs tp ON tr.training_program_id = tp.id
WHERE tr.is_deleted = FALSE
ORDER BY tr.completion_date DESC
LIMIT 10;
```

### View Skills Matrix

```sql
SELECT
    s.id,
    s.employee_id,
    s.skill_name,
    s.proficiency_level,
    s.last_assessed_date,
    s.is_development_area
FROM skills s
WHERE s.employee_id = '{employeeId}'
AND s.is_deleted = FALSE
ORDER BY s.skill_name;
```

### View Mandatory Requirements

```sql
SELECT
    mtr.id,
    tp.name AS training_program_name,
    mtr.department_id,
    mtr.position_id,
    mtr.completion_deadline_days,
    mtr.recertification_months,
    mtr.is_active
FROM mandatory_training_requirements mtr
JOIN training_programs tp ON mtr.training_program_id = tp.id
WHERE mtr.is_active = TRUE
ORDER BY tp.name;
```

## Common Issues & Solutions

### Issue: Migration fails with foreign key error

**Solution**: Ensure training_programs table exists before running migration
```bash
dotnet ef database update --project Maliev.CareerService.Data
```

### Issue: Tests fail with "Docker not running"

**Solution**: Start Docker Desktop before running tests
```bash
docker ps  # Verify Docker is running
```

### Issue: Unauthorized (401) responses

**Solution**: Check JWT token includes required permissions
```bash
# Decode JWT to verify claims
jwt decode $TOKEN
```

### Issue: Background service not running

**Solution**: Check service registration in Program.cs
```csharp
builder.Services.AddHostedService<CertificationExpirationReminderBackgroundService>();
```

### Issue: Events not being consumed

**Solution**: Verify RabbitMQ connection and consumer registration
```bash
# Check RabbitMQ management UI
open http://localhost:15672
# Login: guest/guest
# Check queues and bindings
```

## Development Workflow

### 1. Make Changes

Edit files in:
- `Maliev.CareerService.Data/` - Entities, repositories, commands/queries
- `Maliev.CareerService.Api/` - Controllers, services
- `Maliev.CareerService.Tests/` - Tests

### 2. Run Tests (TDD)

```bash
# Write test first (Red)
dotnet test --filter "TestName"

# Implement feature (Green)
dotnet test --filter "TestName"

# Refactor (Refactor)
dotnet test
```

### 3. Verify Build

```bash
dotnet build --no-incremental
# Must have ZERO warnings
```

### 4. Test Locally

```bash
dotnet run --project Maliev.CareerService.Api
# Test with Postman/curl/Scalar UI
```

### 5. Commit

```bash
git add .
git commit -m "feat: implement training record completion tracking"
```

## Next Steps

After completing local development:

1. Run full test suite: `dotnet test`
2. Verify zero warnings: `dotnet build`
3. Test all API endpoints via Scalar UI
4. Review code coverage (target 80%+)
5. Update API documentation if needed
6. Create pull request to `develop` branch

## Additional Resources

- [spec.md](spec.md) - Feature specification
- [data-model.md](data-model.md) - Entity definitions
- [contracts/](contracts/) - API contracts (OpenAPI)
- [research.md](research.md) - Implementation patterns
- [plan.md](plan.md) - Implementation plan

## Support

For questions or issues:
- Check existing Career Service patterns in codebase
- Review research.md for architectural decisions
- Consult team leads for clarification
