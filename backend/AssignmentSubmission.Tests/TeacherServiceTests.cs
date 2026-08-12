/*
 * File: TeacherServiceTests.cs
 * Purpose: Unit tests validating business rules for Teacher operations (grading validations, unauthorized grading attempts).
 * 
 * Dependencies Used:
 * - TeacherService.cs: Service under test.
 * - ApplicationDbContext.cs: EF Core In-Memory database.
 * - Entities (User, Assignment, Submission, CourseSubject)
 * - xUnit: Testing framework.
 * 
 * Used By:
 * - xUnit Test Runner (`dotnet test`).
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
    public class TeacherServiceTests
    {
        private ApplicationDbContext GetInMemoryDbContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task GradeSubmission_ShouldFail_WhenMarksExceedMaximumAllowed()
        {
            // Arrange
            var context = GetInMemoryDbContext(Guid.NewGuid().ToString());
            var service = new TeacherService(context);

            var teacher = new User { Id = 2, FullName = "Teacher One", Email = "t1@test.com", Role = UserRole.Teacher };
            var student = new User { Id = 1, FullName = "Student One", Email = "s1@test.com", Role = UserRole.Student };
            var course = new Course { Id = 10, Name = "CS 101" };
            var subject = new Subject { Id = 20, Name = "Algorithms" };
            var cs = new CourseSubject { Id = 30, CourseId = 10, SubjectId = 20, TeacherId = 2 };

            var assignment = new Assignment
            {
                Id = 100,
                Title = "Math Homework",
                MaxMarks = 50, // MAX MARKS IS 50
                Deadline = DateTime.UtcNow.AddDays(1),
                Status = AssignmentStatus.Published,
                CourseSubjectId = 30,
                CourseSubject = cs
            };

            var submission = new Submission
            {
                Id = 500,
                AssignmentId = 100,
                StudentId = 1,
                AnswerText = "My math solution",
                Assignment = assignment,
                Student = student
            };

            context.Users.AddRange(teacher, student);
            context.CourseSubjects.Add(cs);
            context.Assignments.Add(assignment);
            context.Submissions.Add(submission);
            await context.SaveChangesAsync();

            var gradeDto = new GradeSubmissionDto
            {
                Marks = 75, // EXCEEDS MAX MARKS OF 50
                Feedback = "Great effort!"
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                service.GradeSubmissionAsync(teacherId: 2, submissionId: 500, gradeDto)
            );

            Assert.Contains("Marks cannot exceed maximum assignment marks", exception.Message);
        }

        [Fact]
        public async Task GradeSubmission_ShouldFail_WhenTeacherIsNotOwnerOfCourseSubject()
        {
            // Arrange
            var context = GetInMemoryDbContext(Guid.NewGuid().ToString());
            var service = new TeacherService(context);

            var realTeacher = new User { Id = 2, FullName = "Real Teacher", Email = "real@test.com", Role = UserRole.Teacher };
            var unauthorizedTeacherId = 99; // Different teacher trying to grade
            var student = new User { Id = 1, FullName = "Student One", Email = "s1@test.com", Role = UserRole.Student };

            var cs = new CourseSubject { Id = 30, CourseId = 10, SubjectId = 20, TeacherId = 2 };

            var assignment = new Assignment
            {
                Id = 100,
                Title = "Physics Assignment",
                MaxMarks = 100,
                Deadline = DateTime.UtcNow.AddDays(1),
                Status = AssignmentStatus.Published,
                CourseSubjectId = 30,
                CourseSubject = cs
            };

            var submission = new Submission
            {
                Id = 500,
                AssignmentId = 100,
                StudentId = 1,
                AnswerText = "Physics solution",
                Assignment = assignment,
                Student = student
            };

            context.Users.AddRange(realTeacher, student);
            context.CourseSubjects.Add(cs);
            context.Assignments.Add(assignment);
            context.Submissions.Add(submission);
            await context.SaveChangesAsync();

            var gradeDto = new GradeSubmissionDto
            {
                Marks = 90,
                Feedback = "Well done"
            };

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                service.GradeSubmissionAsync(teacherId: unauthorizedTeacherId, submissionId: 500, gradeDto)
            );
        }
    }
}
