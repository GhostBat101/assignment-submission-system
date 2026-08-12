/*
 * File: AuthController.cs
 * Purpose: REST API Controller managing authentication endpoints (`/api/auth/login` and `/api/auth/me`).
 * 
 * Dependencies Used:
 * - IAuthService.cs: Authentication business logic.
 * - AuthDtos.cs: Request/Response DTO contracts.
 * - System.Security.Claims: Extracts user identity claims from JWT tokens.
 * 
 * Used By:
 * - Next.js Frontend App: To log in and fetch current authenticated user profile.
 * - Swagger/OpenAPI: API documentation.
 */

using System.Security.Claims;
using System.Threading.Tasks;
using AssignmentSubmission.Core.DTOs;
using AssignmentSubmission.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssignmentSubmission.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Authenticates user credentials and returns a JWT token.
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _authService.LoginAsync(request);
            if (response == null)
            {
                return Unauthorized(new { message = "Invalid email or password." });
            }

            return Ok(response);
        }

        /// <summary>
        /// Retrieves current authenticated user profile from JWT token claims.
        /// </summary>
        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { message = "Invalid authentication token." });
            }

            var profile = await _authService.GetUserProfileAsync(userId);
            if (profile == null)
            {
                return NotFound(new { message = "User profile not found." });
            }

            return Ok(profile);
        }
    }
}
