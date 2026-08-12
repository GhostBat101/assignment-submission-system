/*
 * File: SecurityAndEdgeCaseTests.cs
 * Purpose: Deep edge case testing for boundary conditions, draft privacy, exception middleware handling, and grading lockouts.
 * 
 * Dependencies Used:
 * - StudentService, TeacherService, AdminService
 * - GlobalExceptionMiddleware, SecurityHeadersMiddleware
 * - xUnit
 */

using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using AssignmentSubmission.Api.Middlewares;
using AssignmentSubmission.Core.DTOs;
using AssignmentSubmission.Core.Entities;
using AssignmentSubmission.Infrastructure.Data;
using AssignmentSubmission.Services.Implementations;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace AssignmentSubmission.Tests
{
    public class SecurityAndEdgeCaseTests
    {
        private ApplicationDbContext GetInMemoryDbContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task SubmitAssignment_ShouldFail_WhenAssignmentIsDraft()
        {
            // Arrange
            var context = GetInMemoryDbContext(Guid.NewGuid().ToString());
            var service = new StudentService(context);

            var student = new User { Id = 1, FullName = "Student One", Email = "s1@test.com", Role = UserRole.Student };
            var course = new Course { Id = 10, Name = "CS 101" };
            var cs = new CourseSubject { Id = 30, CourseId = 10, SubjectId = 20, TeacherId = 2 };

            var draftAssignment = new Assignment
            {
                Id = 100,
                Title = "Draft Assignment",
                MaxMarks = 100,
                Deadline = DateTime.UtcNow.AddDays(2),
                Status = AssignmentStatus.Draft, // STATUS IS DRAFT
                CourseSubjectId = 30
            };

            context.Users.Add(student);
            context.Courses.Add(course);
            context.CourseSubjects.Add(cs);
            context.StudentCourses.Add(new StudentCourse { StudentId = 1, CourseId = 10 });
            context.Assignments.Add(draftAssignment);
            await context.SaveChangesAsync();

            var dto = new CreateSubmissionDto { AssignmentId = 100, AnswerText = "Trying to submit to draft" };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                service.SubmitAssignmentAsync(studentId: 1, dto)
            );
            Assert.Contains("published", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task GradeSubmission_ShouldFail_WhenMarksAreNegative()
        {
            // Arrange
            var context = GetInMemoryDbContext(Guid.NewGuid().ToString());
            var service = new TeacherService(context);

            var teacher = new User { Id = 2, FullName = "Teacher", Email = "t@test.com", Role = UserRole.Teacher };
            var student = new User { Id = 1, FullName = "Student", Email = "s@test.com", Role = UserRole.Student };
            var cs = new CourseSubject { Id = 30, CourseId = 10, SubjectId = 20, TeacherId = 2 };
            var assignment = new Assignment { Id = 100, MaxMarks = 100, CourseSubjectId = 30, CourseSubject = cs };
            var submission = new Submission { Id = 500, AssignmentId = 100, StudentId = 1, Assignment = assignment, Student = student };

            context.Users.AddRange(teacher, student);
            context.CourseSubjects.Add(cs);
            context.Assignments.Add(assignment);
            context.Submissions.Add(submission);
            await context.SaveChangesAsync();

            var gradeDto = new GradeSubmissionDto { Marks = -10, Feedback = "Negative marks" };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
                service.GradeSubmissionAsync(teacherId: 2, submissionId: 500, gradeDto)
            );
            Assert.Contains("cannot be negative", ex.Message);
        }

        [Fact]
        public async Task GlobalExceptionMiddleware_ShouldSanitizeExceptionMessage()
        {
            // Arrange
            var logger = NullLogger<GlobalExceptionMiddleware>.Instance;
            RequestDelegate next = (HttpContext ctx) => throw new Exception("Sensitive Database Connection String Leaked!");
            var middleware = new GlobalExceptionMiddleware(next, logger);

            var httpContext = new DefaultHttpContext();
            httpContext.Response.Body = new MemoryStream();

            // Act
            await middleware.InvokeAsync(httpContext);

            // Assert
            Assert.Equal(500, httpContext.Response.StatusCode);
            httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(httpContext.Response.Body);
            var responseText = await reader.ReadToEndAsync();

            Assert.DoesNotContain("Sensitive Database Connection String Leaked!", responseText);
            Assert.Contains("internal server error occurred", responseText);
        }

        [Fact]
        public async Task SecurityHeadersMiddleware_ShouldInjectRequiredHeaders()
        {
            // Arrange
            RequestDelegate next = (HttpContext ctx) => Task.CompletedTask;
            var middleware = new SecurityHeadersMiddleware(next);
            var httpContext = new DefaultHttpContext();

            // Act
            await middleware.InvokeAsync(httpContext);

            // Assert
            Assert.Equal("nosniff", httpContext.Response.Headers["X-Content-Type-Options"]);
            Assert.Equal("DENY", httpContext.Response.Headers["X-Frame-Options"]);
            Assert.Equal("1; mode=block", httpContext.Response.Headers["X-XSS-Protection"]);
        }
    }
}
