/*
 * File: StudentServiceTests.cs
 * Purpose: Unit tests validating business rules for Student assignment submissions, deadline enforcement, and pre-deadline updates.
 * 
 * Dependencies Used:
 * - StudentService.cs
 * - ApplicationDbContext.cs (EF Core In-Memory database)
 * - AssignmentSubmission.Core.Entities
 * - xUnit & Fluent Assertions
 * 
 * Used By:
 * - xUnit Test Runner (`dotnet test`)
 */

using System;
using System.Threading.Tasks;
using AssignmentSubmission.Core.DTOs;
using AssignmentSubmission.Core.Entities;
using AssignmentSubmission.Infrastructure.Data;
using AssignmentSubmission.Services.Implementations;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AssignmentSubmission.Tests
{
    public class StudentServiceTests
    {
        private ApplicationDbContext GetInMemoryDbContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task SubmitAssignment_ShouldFail_WhenDeadlineHasPassed()
        {
            // Arrange
            var context = GetInMemoryDbContext(Guid.NewGuid().ToString());
            var service = new StudentService(context);

            var student = new User { Id = 1, FullName = "Student One", Email = "s1@test.com", Role = UserRole.Student };
            var course = new Course { Id = 10, Name = "CS 101" };
            var subject = new Subject { Id = 20, Name = "Algorithms" };
            var teacher = new User { Id = 2, FullName = "Teacher One", Email = "t1@test.com", Role = UserRole.Teacher };
            var cs = new CourseSubject { Id = 30, CourseId = 10, SubjectId = 20, TeacherId = 2 };

            var expiredAssignment = new Assignment
            {
                Id = 100,
                Title = "Past Homework",
                MaxMarks = 100,
                Deadline = DateTime.UtcNow.AddHours(-2), // EXPIRED DEADLINE
                Status = AssignmentStatus.Published,
                CourseSubjectId = 30
            };

            context.Users.AddRange(student, teacher);
            context.Courses.Add(course);
            context.Subjects.Add(subject);
            context.CourseSubjects.Add(cs);
            context.StudentCourses.Add(new StudentCourse { StudentId = 1, CourseId = 10 });
            context.Assignments.Add(expiredAssignment);
            await context.SaveChangesAsync();

            var submissionDto = new CreateSubmissionDto
            {
                AssignmentId = 100,
                AnswerText = "Late answer submission attempt."
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.SubmitAssignmentAsync(studentId: 1, submissionDto)
            );

            Assert.Contains("deadline has already passed", exception.Message);
        }

        [Fact]
        public async Task SubmitAssignment_ShouldFail_WhenStudentIsNotEnrolledInCourse()
        {
            // Arrange
            var context = GetInMemoryDbContext(Guid.NewGuid().ToString());
            var service = new StudentService(context);

            var student = new User { Id = 1, FullName = "Student One", Email = "s1@test.com", Role = UserRole.Student };
            var course = new Course { Id = 10, Name = "CS 101" };
            var cs = new CourseSubject { Id = 30, CourseId = 10, SubjectId = 20, TeacherId = 2 };

            var validAssignment = new Assignment
            {
                Id = 100,
                Title = "Active Homework",
                MaxMarks = 100,
                Deadline = DateTime.UtcNow.AddDays(2),
                Status = AssignmentStatus.Published,
                CourseSubjectId = 30
            };

            context.Users.Add(student);
            context.Courses.Add(course);
            context.CourseSubjects.Add(cs);
            context.Assignments.Add(validAssignment);
            await context.SaveChangesAsync(); // Note: Student is NOT enrolled in StudentCourses

            var submissionDto = new CreateSubmissionDto
            {
                AssignmentId = 100,
                AnswerText = "Valid answer text"
            };

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                service.SubmitAssignmentAsync(studentId: 1, submissionDto)
            );
        }

        [Fact]
        public async Task SubmitAssignment_ShouldSucceed_WhenValidAndBeforeDeadline()
        {
            // Arrange
            var context = GetInMemoryDbContext(Guid.NewGuid().ToString());
            var service = new StudentService(context);

            var student = new User { Id = 1, FullName = "Student One", Email = "s1@test.com", Role = UserRole.Student };
            var course = new Course { Id = 10, Name = "CS 101" };
            var subject = new Subject { Id = 20, Name = "Algorithms" };
            var teacher = new User { Id = 2, FullName = "Teacher One", Email = "t1@test.com", Role = UserRole.Teacher };
            var cs = new CourseSubject { Id = 30, CourseId = 10, SubjectId = 20, TeacherId = 2, Course = course, Subject = subject, Teacher = teacher };

            var validAssignment = new Assignment
            {
                Id = 100,
                Title = "Active Homework",
                MaxMarks = 100,
                Deadline = DateTime.UtcNow.AddDays(2),
                Status = AssignmentStatus.Published,
                CourseSubjectId = 30,
                CourseSubject = cs
            };

            context.Users.AddRange(student, teacher);
            context.Courses.Add(course);
            context.Subjects.Add(subject);
            context.CourseSubjects.Add(cs);
            context.StudentCourses.Add(new StudentCourse { StudentId = 1, CourseId = 10, Student = student, Course = course });
            context.Assignments.Add(validAssignment);
            await context.SaveChangesAsync();

            var submissionDto = new CreateSubmissionDto
            {
                AssignmentId = 100,
                AnswerText = "Clean, valid student submission answer."
            };

            // Act
            var result = await service.SubmitAssignmentAsync(studentId: 1, submissionDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Clean, valid student submission answer.", result.AnswerText);
            Assert.Equal(1, result.StudentId);
            Assert.Equal("Submitted", result.Status);
        }
    }
}
