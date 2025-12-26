# Maliev.CareerService Development Guidelines

Auto-generated from all feature plans. Last updated: 2025-12-21

## Active Technologies
- .NET 10.0 (C# 14)
- PostgreSQL (Npgsql)
- Redis
- RabbitMQ (MassTransit)
- .NET Aspire ServiceDefaults

## Project Structure (Flat)
- `Maliev.CareerService.Api/`
- `Maliev.CareerService.Data/`
- `Maliev.CareerService.Tests/`

## Constraints & Standards (NON-NEGOTIABLE)
- **NO FluentValidation**: Use standard .NET DataAnnotations or manual logic.
- **NO FluentAssertions**: Use standard xUnit `Assert`.
- **NO AutoMapper**: Use explicit mapping only.
- **Zero Warnings**: Warnings are treated as errors.
- **Test-First**: Write tests before implementation.

## Commands
- Build: `dotnet build`
- Test: `dotnet test`
- Database Update: `dotnet ef database update --project Maliev.CareerService.Data --startup-project Maliev.CareerService.Api`

## Recent Changes
- 002-iam-integration: Implemented permission-based authorization and IAM registration.
- Migration: Upgraded to .NET 10.0 and enforced Constitution library standards (removed FluentValidation/Assertions).

<!-- MANUAL ADDITIONS START -->
<!-- MANUAL ADDITIONS END -->