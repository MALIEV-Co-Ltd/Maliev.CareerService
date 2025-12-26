namespace Maliev.CareerService.Api.Services.External;

/// <summary>
/// Service client for Identity Access Management (IAM) registration.
/// </summary>
public interface IIamServiceClient
{
    /// <summary>
    /// Registers a service manifest containing permissions and roles with the central IAM.
    /// </summary>
    Task RegisterManifestAsync(IamManifest manifest, CancellationToken cancellationToken = default);
}

/// <summary>Represents the full IAM manifest for a service.</summary>
/// <param name="ServiceName">The unique name of the service.</param>
/// <param name="Permissions">The list of granular permissions.</param>
/// <param name="Roles">The list of predefined roles.</param>
public record IamManifest(
    string ServiceName,
    List<IamPermission> Permissions,
    List<IamRole> Roles);

/// <summary>Represents a single granular permission.</summary>
/// <param name="Name">The permission string (e.g., career.trainings.read).</param>
/// <param name="Description">A human-readable description of the permission.</param>
public record IamPermission(string Name, string Description);

/// <summary>Represents a predefined role mapping.</summary>
/// <param name="Name">The unique role name.</param>
/// <param name="Permissions">The list of permission strings assigned to the role.</param>
public record IamRole(string Name, List<string> Permissions);