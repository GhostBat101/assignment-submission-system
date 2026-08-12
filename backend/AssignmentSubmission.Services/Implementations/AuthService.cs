/*
 * File: AuthService.cs
 * Purpose: Implements authentication business logic including credentials verification and profile data formatting.
 * 
 * Dependencies Used:
 * - ApplicationDbContext.cs: Database access to fetch User records.
 * - PasswordHasher.cs: Cryptographic verification of submitted passwords against stored hashes.
 * - IJwtTokenGenerator.cs: Generates signed JWT tokens.
 * - AuthDtos.cs: LoginRequestDto, LoginResponseDto, UserDto payloads.
 * 
 * Used By:
 * - AuthController.cs: Handles `/api/auth/login` business rules.
 * - Program.cs: Dependency Injection registration.
 */

using System.Threading.Tasks;
using AssignmentSubmission.Core.DTOs;
using AssignmentSubmission.Infrastructure.Data;
using AssignmentSubmission.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AssignmentSubmission.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;

        public AuthService(ApplicationDbContext context, IJwtTokenGenerator jwtTokenGenerator)
        {
            _context = context;
            _jwtTokenGenerator = jwtTokenGenerator;
        }

        public async Task<LoginResponseDto?> LoginAsync(LoginRequestDto request)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower());

            if (user == null)
            {
                return null; // User not found
            }

            bool isPasswordValid = PasswordHasher.VerifyPassword(request.Password, user.PasswordHash);
            if (!isPasswordValid)
            {
                return null; // Invalid credentials
            }

            var token = _jwtTokenGenerator.GenerateToken(user);

            var userDto = new UserDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role.ToString()
            };

            return new LoginResponseDto
            {
                Token = token,
                User = userDto
            };
        }

        public async Task<UserDto?> GetUserProfileAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return null;

            return new UserDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role.ToString()
            };
        }
    }
}
