# Maliev.CareerService

Career Service is a RESTful API service for managing job positions, applications, and related career information for Maliev Co. Ltd.

## Description

This service provides a comprehensive API for managing job positions, applications, skills, and work locations within the Maliev organization. It includes features like job position creation and management, application processing, skill categorization, and work location tracking.

## Technologies Used

- **.NET 9** - Latest version of the .NET framework
- **ASP.NET Core** - Web API framework
- **Entity Framework Core** - ORM for data access
- **PostgreSQL** - Production database
- **In-Memory Database** - Development and testing database
- **Swagger/OpenAPI** - API documentation and testing interface
- **Serilog** - Structured logging
- **xUnit** - Unit testing framework
- **Moq** - Mocking framework for tests
- **FluentAssertions** - Fluent API for assertions in tests
- **Docker** - Containerization platform

## Getting Started

### Prerequisites

- .NET 9 SDK
- Docker (for running PostgreSQL in development)
- IDE (Visual Studio, Visual Studio Code, or JetBrains Rider)

### Building the Project

```bash
dotnet build
```

### Running the Service Locally

1. **With In-Memory Database (Default for Development):**
   ```bash
   cd Maliev.CareerService.Api
   dotnet run
   ```

2. **With PostgreSQL (Using Docker):**
   ```bash
   # Start PostgreSQL container
   docker-compose up -d
   
   # Run the service
   cd Maliev.CareerService.Api
   dotnet run
   ```

### Configuration

The service can be configured through `appsettings.json` files:
- `appsettings.json` - Base configuration
- `appsettings.Development.json` - Development-specific overrides

Key configuration sections include:
- Connection strings for the database
- Rate limiting settings
- Cache options
- Upload service options
- GCS (Google Cloud Storage) configuration
- JWT authentication options

### Running Tests

To run all tests:
```bash
dotnet test
```

The test suite includes:
- Unit tests for all service layers
- Tests covering success and failure scenarios
- In-memory database for test isolation

## API Endpoints

The API is versioned and accessible under the `/careers/v1` base path. All endpoints return JSON responses.

### Job Positions
- `GET /careers/v1/positions/{id}` - Get a specific job position
- `GET /careers/v1/positions/search` - Search job positions with filters
- `GET /careers/v1/positions/public` - Get publicly available positions
- `GET /careers/v1/positions/departments` - Get list of departments
- `POST /careers/v1/positions` - Create a new job position (requires authentication)
- `PUT /careers/v1/positions/{id}` - Update an existing job position (requires authentication)
- `DELETE /careers/v1/positions/{id}` - Delete a job position (requires authentication)
- `GET /careers/v1/positions/{id}/exists` - Check if a job position exists (requires authentication)
- `GET /careers/v1/positions/validate` - Validate job position uniqueness (requires authentication)

### Job Applications
- `GET /careers/v1/applications/{id}` - Get a specific job application (requires authentication)
- `GET /careers/v1/applications/search` - Search job applications with filters (requires authentication)
- `POST /careers/v1/applications` - Create a new job application
- `PUT /careers/v1/applications/{id}` - Update an existing job application (requires authentication)
- `DELETE /careers/v1/applications/{id}` - Delete a job application (requires authentication)

### Skills
- `GET /careers/v1/skills` - Get all skills
- `GET /careers/v1/skills/categories` - Get skill categories
- `POST /careers/v1/skills` - Create a new skill (requires authentication)
- `PUT /careers/v1/skills/{id}` - Update an existing skill (requires authentication)
- `DELETE /careers/v1/skills/{id}` - Delete a skill (requires authentication)

### Work Locations
- `GET /careers/v1/locations` - Get all work locations
- `POST /careers/v1/locations` - Create a new work location (requires authentication)
- `PUT /careers/v1/locations/{id}` - Update an existing work location (requires authentication)
- `DELETE /careers/v1/locations/{id}` - Delete a work location (requires authentication)

### Health Checks
- `GET /careers/liveness` - Basic liveness check
- `GET /careers/readiness` - Detailed readiness check including database and service dependencies
- `GET /careers/metrics` - Prometheus metrics endpoint

## Authentication

The API uses JWT Bearer token authentication for protected endpoints. To access protected endpoints, include an Authorization header with a valid JWT token:

```
Authorization: Bearer <your-jwt-token>
```

## Rate Limiting

The service implements rate limiting to prevent abuse:
- Global rate limiting applied to all endpoints
- Specific rate limiting for career endpoints
- Configurable through appsettings

## Health Monitoring

The service provides health check endpoints for monitoring:
- Liveness probe for basic service availability
- Readiness probe for checking database and service dependencies
- Prometheus metrics endpoint for detailed monitoring

## Development Environment

For local development:
1. The service uses an in-memory database by default when no connection string is provided
2. CORS is configured for maliev.com domains
3. Swagger UI is available at `/careers/swagger`
4. Structured logging is configured for console and file output

## Deployment

The service can be deployed using Docker. A Dockerfile is provided in the `Maliev.CareerService.Api` directory.

For production deployment:
1. Configure appropriate database connection strings
2. Set up JWT authentication secrets
3. Configure environment-specific appsettings
4. Ensure proper security configurations are in place

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Run tests to ensure nothing is broken
5. Submit a pull request

## Support

For support, contact the development team or create an issue in the GitHub repository.