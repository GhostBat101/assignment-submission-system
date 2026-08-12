/*
 * File: AuthServiceTests.cs
 * Purpose: Unit tests validating Authentication, password hashing integrity, and invalid login handling.
 * 
 * Dependencies Used:
 * - AuthService.cs
 * - PasswordHasher.cs
 * - JwtTokenGenerator.cs
 * - ApplicationDbContext.cs
 * - xUnit
 */

using System;
using System.Threading.Tasks;
using AssignmentSubmission.Core.DTOs;
using AssignmentSubmission.Core.Entities;
using AssignmentSubmission.Infrastructure.Data;
using AssignmentSubmission.Services.Implementations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace AssignmentSubmission.Tests
{
    public class AuthServiceTests
    {
        private ApplicationDbContext GetInMemoryDbContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
            return new ApplicationDbContext(options);
        }

        private JwtTokenGenerator GetJwtTokenGenerator()
        {
            var inMemorySettings = new System.Collections.Generic.Dictionary<string, string> {
                {"Jwt:Secret", "SuperSecretKeyForAssignmentSubmissionManagementSystem2026!"},
                {"Jwt:Issuer", "AssignmentSubmissionApi"},
                {"Jwt:Audience", "AssignmentSubmissionApp"}
            };

            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings!)
                .Build();

            return new JwtTokenGenerator(configuration);
        }

        [Fact]
        public async Task LoginAsync_ShouldSucceed_WithValidCredentials()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            var context = GetInMemoryDbContext(dbName);
            var jwtGen = GetJwtTokenGenerator();
            var service = new AuthService(context, jwtGen);

            var passwordHash = PasswordHasher.HashPassword("Password123!");
            var user = new User
            {
                Id = 1,
                FullName = "Test Student",
                Email = "user@test.com",
                PasswordHash = passwordHash,
                Role = UserRole.Student
            };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var loginDto = new LoginRequestDto
            {
                Email = "user@test.com",
                Password = "Password123!"
            };

            // Act
            var result = await service.LoginAsync(loginDto);

            // Assert
            Assert.NotNull(result);
            Assert.False(string.IsNullOrEmpty(result.Token));
            Assert.Equal("user@test.com", result.User.Email);
            Assert.Equal("Student", result.User.Role);
        }

        [Fact]
        public async Task LoginAsync_ShouldReturnNull_WithIncorrectPassword()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            var context = GetInMemoryDbContext(dbName);
            var jwtGen = GetJwtTokenGenerator();
            var service = new AuthService(context, jwtGen);

            var passwordHash = PasswordHasher.HashPassword("Password123!");
            context.Users.Add(new User
            {
                Id = 1,
                FullName = "Test Student",
                Email = "user@test.com",
                PasswordHash = passwordHash,
                Role = UserRole.Student
            });
            await context.SaveChangesAsync();

            var loginDto = new LoginRequestDto
            {
                Email = "user@test.com",
                Password = "WrongPassword!"
            };

            // Act
            var result = await service.LoginAsync(loginDto);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task LoginAsync_ShouldReturnNull_WhenUserDoesNotExist()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            var context = GetInMemoryDbContext(dbName);
            var jwtGen = GetJwtTokenGenerator();
            var service = new AuthService(context, jwtGen);

            var loginDto = new LoginRequestDto
            {
                Email = "nonexistent@test.com",
                Password = "Password123!"
            };

            // Act
            var result = await service.LoginAsync(loginDto);

            // Assert
            Assert.Null(result);
        }
    }
}
