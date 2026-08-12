/*
 * File: AuthDtos.cs
 * Purpose: Data Transfer Objects (DTOs) for authenticating users, transferring credentials, and returning JWT tokens.
 * 
 * Dependencies Used:
 * - Enums.cs: UserRole enum for role identification.
 * 
 * Used By:
 * - AuthController.cs: Receives client requests and returns standardized JSON outputs.
 * - AuthService.cs: Uses DTOs for processing authentication logic.
 */

using System.ComponentModel.DataAnnotations;
using AssignmentSubmission.Core.Entities;

namespace AssignmentSubmission.Core.DTOs
{
    public class LoginRequestDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }

    public class UserDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }

    public class LoginResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public UserDto User { get; set; } = null!;
    }
}
