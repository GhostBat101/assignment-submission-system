/*
 * File: DbInitializer.cs
 * Purpose: Automatically populates the database with initial demo data (Admin, Teacher, Student, Courses, Assignments).
 * 
 * Dependencies Used:
 * - ApplicationDbContext.cs: Database context.
 * - PasswordHasher.cs: Hashes user credentials.
 * - Entities (User, Course, Subject, CourseSubject, StudentCourse, Assignment): Data models.
 * 
 * Used By:
 * - Program.cs: Executed during API startup to seed missing data.
 */

using System;
using System.Linq;
using AssignmentSubmission.Core.Entities;

namespace AssignmentSubmission.Infrastructure.Data
{
    public static class DbInitializer
    {
        public static void Initialize(ApplicationDbContext context)
        {
            context.Database.EnsureCreated();

            // Look for any existing users
            if (context.Users.Any())
            {
                return; // DB has been seeded
            }

            // 1. Seed Users
            var admin = new User
            {
                FullName = "System Administrator",
                Email = "admin@school.com",
                PasswordHash = PasswordHasher.HashPassword("Admin123!"),
                Role = UserRole.Admin,
                CreatedAt = DateTime.UtcNow
            };

            var teacher = new User
            {
                FullName = "Prof. John Doe",
                Email = "teacher@school.com",
                PasswordHash = PasswordHasher.HashPassword("Teacher123!"),
                Role = UserRole.Teacher,
                CreatedAt = DateTime.UtcNow
            };

            var student = new User
            {
                FullName = "Jane Smith",
                Email = "student@school.com",
                PasswordHash = PasswordHasher.HashPassword("Student123!"),
                Role = UserRole.Student,
                CreatedAt = DateTime.UtcNow
            };

            context.Users.AddRange(admin, teacher, student);
            context.SaveChanges();

            // 2. Seed Courses
            var course1 = new Course
            {
                Name = "Computer Science 101",
                Description = "Introduction to Computer Science and Software Engineering"
            };
            var course2 = new Course
            {
                Name = "Mathematics 201",
                Description = "Advanced Calculus and Algebra"
            };

            context.Courses.AddRange(course1, course2);
            context.SaveChanges();

            // 3. Seed Subjects
            var subject1 = new Subject
            {
                Name = "Data Structures & Algorithms",
                Code = "CS-101"
            };
            var subject2 = new Subject
            {
                Name = "Linear Algebra",
                Code = "MATH-201"
            };

            context.Subjects.AddRange(subject1, subject2);
            context.SaveChanges();

            // 4. Assign Teacher to Subject in Course
            var cs1 = new CourseSubject
            {
                CourseId = course1.Id,
                SubjectId = subject1.Id,
                TeacherId = teacher.Id
            };
            var cs2 = new CourseSubject
            {
                CourseId = course2.Id,
                SubjectId = subject2.Id,
                TeacherId = teacher.Id
            };

            context.CourseSubjects.AddRange(cs1, cs2);
            context.SaveChanges();

            // 5. Enroll Student into Course
            var studentEnrollment = new StudentCourse
            {
                StudentId = student.Id,
                CourseId = course1.Id,
                EnrolledAt = DateTime.UtcNow
            };

            context.StudentCourses.Add(studentEnrollment);
            context.SaveChanges();

            // 6. Seed Sample Assignment
            var assignment1 = new Assignment
            {
                Title = "Arrays & Linked Lists Implementation",
                Description = "Implement a singly linked list with insertion and deletion operations in C# or JavaScript.",
                MaxMarks = 100,
                Deadline = DateTime.UtcNow.AddDays(7),
                Status = AssignmentStatus.Published,
                CourseSubjectId = cs1.Id,
                CreatedAt = DateTime.UtcNow
            };

            var assignment2 = new Assignment
            {
                Title = "Binary Search Tree Draft",
                Description = "Draft assignment for BST insertion.",
                MaxMarks = 50,
                Deadline = DateTime.UtcNow.AddDays(14),
                Status = AssignmentStatus.Draft,
                CourseSubjectId = cs1.Id,
                CreatedAt = DateTime.UtcNow
            };

            context.Assignments.AddRange(assignment1, assignment2);
            context.SaveChanges();
        }
    }
}
