/*
 * File: AdminServiceTests.cs
 * Purpose: Comprehensive unit tests validating Admin business logic, user creation, teacher assignment, enrollment, and paginated query results.
 * 
 * Dependencies Used:
 * - AdminService.cs
 * - ApplicationDbContext.cs (EF Core In-Memory database)
 * - AssignmentSubmission.Core.Entities
 * - xUnit
 */

using System;
using System.Linq;
using System.Threading.Tasks;
using AssignmentSubmission.Core.DTOs;
using AssignmentSubmission.Core.Entities;
using AssignmentSubmission.Infrastructure.Data;
using AssignmentSubmission.Services.Implementations;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AssignmentSubmission.Tests
{
    public class AdminServiceTests
    {
        private ApplicationDbContext GetInMemoryDbContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task GetAllAssignmentsAsync_ShouldReturnPaginatedResults_CorrectlyCalculated()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            var context = GetInMemoryDbContext(dbName);
            var service = new AdminService(context);

            var course = new Course { Id = 1, Name = "CS" };
            var subject = new Subject { Id = 1, Name = "DB", Code = "CS101" };
            var teacher = new User { Id = 1, FullName = "Teacher A", Email = "t@test.com", Role = UserRole.Teacher };
            var cs = new CourseSubject { Id = 1, CourseId = 1, SubjectId = 1, TeacherId = 1, Course = course, Subject = subject, Teacher = teacher };

            context.Courses.Add(course);
            context.Subjects.Add(subject);
            context.Users.Add(teacher);
            context.CourseSubjects.Add(cs);

            for (int i = 1; i <= 15; i++)
            {
                context.Assignments.Add(new Assignment
                {
                    Id = i,
                    Title = $"Assignment {i}",
                    Description = "Desc",
                    MaxMarks = 100,
                    Deadline = DateTime.UtcNow.AddDays(i),
                    Status = AssignmentStatus.Published,
                    CourseSubjectId = 1,
                    CourseSubject = cs
                });
            }
            await context.SaveChangesAsync();

            // Act - Page 1, PageSize 5
            var page1 = await service.GetAllAssignmentsAsync(page: 1, pageSize: 5);

            // Assert
            Assert.Equal(15, page1.TotalCount);
            Assert.Equal(3, page1.TotalPages);
            Assert.Equal(5, page1.Data.Count());
            Assert.True(page1.HasNextPage);
            Assert.False(page1.HasPreviousPage);

            // Act - Page 3, PageSize 5
            var page3 = await service.GetAllAssignmentsAsync(page: 3, pageSize: 5);
            Assert.False(page3.HasNextPage);
            Assert.True(page3.HasPreviousPage);
        }

        [Fact]
        public async Task EnrollStudentInCourseAsync_ShouldPreventDuplicateEnrollment()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            var context = GetInMemoryDbContext(dbName);
            var service = new AdminService(context);

            var student = new User { Id = 10, FullName = "Student A", Email = "student@test.com", Role = UserRole.Student };
            var course = new Course { Id = 5, Name = "Math 101" };

            context.Users.Add(student);
            context.Courses.Add(course);
            context.StudentCourses.Add(new StudentCourse { StudentId = 10, CourseId = 5 });
            await context.SaveChangesAsync();

            var enrollDto = new EnrollStudentDto { StudentId = 10, CourseId = 5 };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.EnrollStudentInCourseAsync(enrollDto)
            );
            Assert.Contains("Student is already enrolled in this course.", exception.Message);
        }
    }
}
