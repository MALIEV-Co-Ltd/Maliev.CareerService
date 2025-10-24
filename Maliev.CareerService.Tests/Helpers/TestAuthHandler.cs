using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace Maliev.CareerService.Tests.Helpers;

/// <summary>
/// Test authentication handler for integration tests
/// </summary>
public class TestAuthHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string AuthenticationScheme = "Test";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Check if test claims are provided
        var authHeader = Context.Request.Headers["Authorization"].ToString();

        if (string.IsNullOrEmpty(authHeader))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        // Extract custom claims from header if provided
        // Format: "Bearer {role} {email}"
        var parts = authHeader.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var email = parts.Length > 2 ? parts[2] : "test@example.com";
        var userId = parts.Length > 3 ? parts[3] : Guid.NewGuid().ToString();

        // Create test claims
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId),
            new("sub", userId),
            new("user_id", userId),
            new(ClaimTypes.Name, "Test User"),
            new(ClaimTypes.Email, email),
            new("email", email)
        };

        // Check for roles in header
        if (authHeader.Contains("admin", StringComparison.OrdinalIgnoreCase))
        {
            claims.Add(new Claim(ClaimTypes.Role, "Admin"));
        }

        if (authHeader.Contains("HRStaff", StringComparison.Ordinal))
        {
            claims.Add(new Claim(ClaimTypes.Role, "HRStaff"));
        }

        if (authHeader.Contains("Applicant", StringComparison.Ordinal))
        {
            claims.Add(new Claim(ClaimTypes.Role, "Applicant"));
        }

        var identity = new ClaimsIdentity(claims, AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, AuthenticationScheme);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
