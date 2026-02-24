# Research: Permission-Based Authorization & IAM Integration

## Unknowns & Clarifications

### 1. Permission Enforcement Pattern
**Question**: What is the preferred pattern for enforcing granular permissions in MALIEV services?
- **Option A**: Custom `AuthorizeAttribute` or `Policy` for each permission (e.g., `[HasPermission(CareerPermissions.Trainings.Create)]`).
- **Option B**: Use standard ASP.NET Core Roles mapping to permissions at token issuance (less granular).
- **Option C**: Custom `AuthorizationHandler` that checks a `permissions` claim in the JWT.

**Decision**: Option C. We will assume the JWT contains a `permissions` claim. We will implement a custom `AuthorizationHandler` and a declarative attribute for ease of use.

### 2. IAM Service Integration
**Question**: How are permissions and roles "registered" with the central IAM?
- **Pattern**: Most MALIEV services likely have a background service or startup task that sends a manifest of defined permissions/roles to an `IdentityService` or `IAMService`.
- **Action**: Implement `CareerIAMRegistrationService` as an `IHostedService` that calls the IAM API on startup.

### 3. ServiceDefaults NuGet Migration
**Question**: How to resolve the forbidden project reference to `Maliev.Aspire.ServiceDefaults`?
- **Action**: Update `csproj` to use `PackageReference` and ensure `nuget.config` is correctly configured with GitHub Packages credentials.

## Best Practices

### Permission Constants
- Use a nested class structure for permissions: `CareerPermissions.Trainings.Create`.
- This provides better IntelliSense and organization than a flat list of strings.

### Performance
- Permission checks should be purely claim-based to avoid database lookups on every request, meeting the <10ms overhead goal.

## Integration Patterns

### Controller Updates
- Every action method should have an explicit permission check.
- Use `[Authorize(Policy = "...")]` or a custom `[HasPermission(...)]`.
