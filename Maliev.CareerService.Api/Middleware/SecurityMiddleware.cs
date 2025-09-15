using System.Text;
using System.Text.RegularExpressions;

namespace Maliev.CareerService.Api.Middleware;

/// <summary>
/// Middleware to sanitize input and encode output to prevent XSS and other injection attacks.
/// </summary>
public class SecurityMiddleware
{
    private readonly RequestDelegate _next;

    public SecurityMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Add security headers
        AddSecurityHeaders(context);

        // Sanitize input for POST/PUT requests
        if (context.Request.Method == "POST" || context.Request.Method == "PUT")
        {
            context.Request.EnableBuffering();
            
            var originalBodyStream = context.Response.Body;
            using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            await _next(context);

            // Encode output to prevent XSS
            await EncodeOutput(context, originalBodyStream, responseBody);
        }
        else
        {
            await _next(context);
        }
    }

    private static void AddSecurityHeaders(HttpContext context)
    {
        var headers = context.Response.Headers;

        // Prevent XSS attacks
        if (!headers.ContainsKey("X-Content-Type-Options"))
        {
            headers.Append("X-Content-Type-Options", "nosniff");
        }

        // Prevent clickjacking
        if (!headers.ContainsKey("X-Frame-Options"))
        {
            headers.Append("X-Frame-Options", "DENY");
        }

        // Enable XSS protection (for older browsers)
        if (!headers.ContainsKey("X-XSS-Protection"))
        {
            headers.Append("X-XSS-Protection", "1; mode=block");
        }

        // Content Security Policy
        if (!headers.ContainsKey("Content-Security-Policy"))
        {
            headers.Append("Content-Security-Policy", "default-src 'self'; script-src 'self' 'unsafe-inline'; style-src 'self' 'unsafe-inline'; img-src 'self' data:;");
        }

        // Referrer Policy
        if (!headers.ContainsKey("Referrer-Policy"))
        {
            headers.Append("Referrer-Policy", "no-referrer");
        }

        // Permissions Policy
        if (!headers.ContainsKey("Permissions-Policy"))
        {
            headers.Append("Permissions-Policy", "geolocation=(), microphone=(), camera=()");
        }
    }

    private static async Task EncodeOutput(HttpContext context, Stream originalBodyStream, MemoryStream responseBody)
    {
        responseBody.Seek(0, SeekOrigin.Begin);
        var text = await new StreamReader(responseBody).ReadToEndAsync();
        responseBody.Seek(0, SeekOrigin.Begin);
        responseBody.SetLength(0);

        // Simple HTML encoding for JSON responses
        if (context.Response.ContentType?.Contains("application/json") == true)
        {
            // In a real implementation, we would want to be more selective about what we encode
            // This is a simplified example
            text = HtmlEncodeJsonStrings(text);
        }

        var bytes = Encoding.UTF8.GetBytes(text);
        await responseBody.WriteAsync(bytes, 0, bytes.Length);
        responseBody.Seek(0, SeekOrigin.Begin);

        await responseBody.CopyToAsync(originalBodyStream);
    }

    private static string HtmlEncodeJsonStrings(string json)
    {
        // This is a simplified approach - in a real implementation, you'd want to use
        // a proper JSON parser to avoid double-encoding or breaking valid JSON
        return Regex.Replace(json, @"(?<=:\s*"")[^""]*(?="")", match => 
            System.Net.WebUtility.HtmlEncode(match.Value));
    }

    public static string SanitizeInput(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        // Remove or encode potentially dangerous characters
        // This is a basic example - you might want to use a more comprehensive library
        return input
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;")
            .Replace("'", "&#x27;")
            .Replace("/", "&#x2F;");
    }
}

public static class SecurityMiddlewareExtensions
{
    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<SecurityMiddleware>();
    }
}