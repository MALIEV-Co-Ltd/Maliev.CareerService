# Career Service Tests

## Test Structure

- **Unit Tests** (`Unit/`): 403 tests using InMemoryDatabase - Always runnable, no dependencies
- **Integration Tests** (`Integration/`): API endpoint tests with real PostgreSQL - Optional
- **Contract Tests** (`Contract/`): OpenAPI contract validation - Optional

## Running Tests

### Quick Start (Unit Tests Only)

```bash
dotnet test --filter "FullyQualifiedName~Unit"
```

**Result**: 402 passing, 1 skipped (concurrency test has known InMemoryDatabase limitation)

### Running All Tests (Including Integration)

Integration tests require PostgreSQL. You have several options:

#### Option 1: Local PostgreSQL Install

1. Install PostgreSQL locally
2. Create test database:
   ```sql
   CREATE DATABASE career_test_db;
   ```
3. Set environment variable:
   ```bash
   # Windows PowerShell
   $env:TEST_DATABASE_URL="Server=localhost;Port=5432;Database=career_test_db;User Id=postgres;Password=yourpassword"

   # Windows CMD
   set TEST_DATABASE_URL=Server=localhost;Port=5432;Database=career_test_db;User Id=postgres;Password=yourpassword

   # Linux/Mac
   export TEST_DATABASE_URL="Server=localhost;Port=5432;Database=career_test_db;User Id=postgres;Password=yourpassword"
   ```
4. Run tests:
   ```bash
   dotnet test
   ```

#### Option 2: Docker PostgreSQL (Optional)

```bash
# Start PostgreSQL in Docker
docker run -d --name career-test-db \
  -e POSTGRES_PASSWORD=test123 \
  -e POSTGRES_DB=career_test_db \
  -p 5432:5432 \
  postgres:18

# Set environment variable
export TEST_DATABASE_URL="Server=localhost;Port=5432;Database=career_test_db;User Id=postgres;Password=test123"

# Run tests
dotnet test

# Cleanup
docker stop career-test-db
docker rm career-test-db
```

#### Option 3: Cloud PostgreSQL

Use any PostgreSQL provider (AWS RDS, Azure, etc.):

```bash
export TEST_DATABASE_URL="Server=your-db.amazonaws.com;Port=5432;Database=career_test_db;User Id=testuser;Password=secret"
dotnet test
```

## CI/CD (GitHub Actions)

Integration tests run automatically in GitHub Actions using PostgreSQL service container:

```yaml
services:
  postgres:
    image: postgres:18
    env:
      POSTGRES_PASSWORD: test123
      POSTGRES_DB: career_test_db
    ports:
      - 5432:5432
```

Environment variable is set in workflow: `TEST_DATABASE_URL=Server=localhost;Port=5432;...`

## What Happens Without PostgreSQL?

- Unit tests run normally ✅
- Integration tests are **skipped** with informative message ⏭️
- No failures or errors ✅
- Message displayed: "PostgreSQL not configured. Set TEST_DATABASE_URL environment variable..."

## Test Coverage

- **Unit Tests**: Business logic, validators, services (fast, no dependencies)
- **Integration Tests**: Full API workflows with database (comprehensive, requires PostgreSQL)
- **Contract Tests**: API schema validation (requires PostgreSQL)

**Recommended Local Development**: Run unit tests only (`--filter FullyQualifiedName~Unit`)
**CI/CD Pipeline**: Run all tests with PostgreSQL service container
