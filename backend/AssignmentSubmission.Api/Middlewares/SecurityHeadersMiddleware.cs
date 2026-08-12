/*
 * File: SecurityHeadersMiddleware.cs
 * Purpose: Injects HTTP security headers (Helmet equivalents) into all responses to mitigate XSS, Clickjacking, and Sniffing.
 * 
 * Dependencies Used:
 * - Microsoft.AspNetCore.Http: Intercepts outgoing response headers.
 * 
 * Used By:
 * - Program.cs: Registered in the HTTP pipeline.
 */

using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace AssignmentSubmission.Api.Middlewares
{
    public class SecurityHeadersMiddleware
    {
        private readonly RequestDelegate _next;

        public SecurityHeadersMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Add security headers to the response
            context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
            context.Response.Headers.Append("X-Frame-Options", "DENY");
            context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
            context.Response.Headers.Append("Strict-Transport-Security", "max-age=31536000; includeSubDomains");

            await _next(context);
        }
    }
}
