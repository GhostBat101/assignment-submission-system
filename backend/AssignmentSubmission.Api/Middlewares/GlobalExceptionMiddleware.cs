/*
 * File: GlobalExceptionMiddleware.cs
 * Purpose: Catches all unhandled exceptions globally to prevent application crashes and sanitize error responses.
 * 
 * Dependencies Used:
 * - Microsoft.AspNetCore.Http: For intercepting HTTP context.
 * - Microsoft.Extensions.Logging: For structured logging of actual exceptions.
 * 
 * Used By:
 * - Program.cs: Registered at the top of the HTTP pipeline.
 */

using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace AssignmentSubmission.Api.Middlewares
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception has occurred while executing the request.");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            // In production, we NEVER return the raw exception message/stack trace to prevent info leakage.
            var response = new
            {
                message = "An internal server error occurred. Our team has been notified.",
                statusCode = context.Response.StatusCode
            };

            var jsonResponse = JsonSerializer.Serialize(response);
            return context.Response.WriteAsync(jsonResponse);
        }
    }
}
