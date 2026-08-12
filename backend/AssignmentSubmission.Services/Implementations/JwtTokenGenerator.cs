/*
 * File: JwtTokenGenerator.cs
 * Purpose: Implements JWT token creation, signing it with a secret key and embedding user identity and role claims.
 * 
 * Dependencies Used:
 * - IJwtTokenGenerator.cs: Interface definition.
 * - User.cs: Domain entity for claims.
 * - Microsoft.Extensions.Configuration: Retrieves secret key, issuer, and audience settings.
 * - System.IdentityModel.Tokens.Jwt & Microsoft.IdentityModel.Tokens: Cryptographic token construction.
 * 
 * Used By:
 * - AuthService.cs: Called after password verification to produce JWT string.
 * - Program.cs: Registered for Dependency Injection.
 */

using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AssignmentSubmission.Core.Entities;
using AssignmentSubmission.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace AssignmentSubmission.Services.Implementations
{
    public class JwtTokenGenerator : IJwtTokenGenerator
    {
        private readonly IConfiguration _configuration;

        public JwtTokenGenerator(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerateToken(User user)
        {
            var secretKey = _configuration["Jwt:Secret"] ?? "SuperSecretKeyForAssignmentSubmissionManagementSystem2026!";
            var issuer = _configuration["Jwt:Issuer"] ?? "AssignmentSubmissionApi";
            var audience = _configuration["Jwt:Audience"] ?? "AssignmentSubmissionApp";

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddDays(7),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
