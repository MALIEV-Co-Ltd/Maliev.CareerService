# Quickstart: Permission-Based Authorization

This document describes how to use and maintain the permission-based authorization system in the Career Service.

## Key Components

### 1. Permission Constants
All permissions are defined as nested constants in `Maliev.CareerService.Api.Authentication.CareerPermissions`.
Example: `CareerPermissions.Trainings.Read` maps to `career.trainings.read`.

### 2. Authorization in Controllers
Endpoints are secured using the standard `[Authorize]` attribute with the `Policy` property.
You do **not** need to register policies manually; the `PermissionPolicyProvider` handles any policy name starting with `career.`.

```csharp
[HttpGet]
[Authorize(Policy = CareerPermissions.Trainings.Read)]
public async Task<ActionResult> GetTrainings() { ... }
```

### 3. Predefined Roles
Roles are logical groupings of permissions defined in `CareerPredefinedRoles.cs`. These are registered with the central IAM service on startup.

### 4. IAM Registration
The `CareerIAMRegistrationService` (a background service) automatically sends the service manifest (all permissions and roles) to the central IAM service when the application starts.

## Integration Testing
To test authorized endpoints, use the `CreateTestJwtToken` method in your test factory and pass the required permissions in the `permissions` parameter.

```csharp
var token = _factory.CreateTestJwtToken(
    userId: "test-user",
    roles: null,
    permissions: new[] { CareerPermissions.Trainings.Read }
);
```

## Performance
Authorization checks are purely claim-based and performed in-memory, ensuring minimal latency overhead (<10ms).