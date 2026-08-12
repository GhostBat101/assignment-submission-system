/*
 * File: AdminDtos.cs
 * Purpose: Data Transfer Objects for Admin tasks: User management, Course creation, Subject creation, Teacher assignments, and Student enrollments.
 * 
 * Dependencies Used:
 * - System.ComponentModel.DataAnnotations: Input validation attributes.
 * - Enums.cs: UserRole enum.
 * 
 * Used By:
 * - AdminController.cs: Accepts Admin input data and formats output responses.
 * - AdminService.cs: Processes administrative operations.
 */

using System.ComponentModel.DataAnnotations;
using AssignmentSubmission.Core.Entities;

namespace AssignmentSubmission.Core.DTOs
{
    public class CreateUserDto
    {
        [Required]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;

        [Required]
        public UserRole Role { get; set; }
    }

    public class CreateCourseDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;
    }

    public class CreateSubjectDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Code { get; set; } = string.Empty;
    }

    public class AssignTeacherDto
    {
        [Required]
        public int CourseId { get; set; }

        [Required]
        public int SubjectId { get; set; }

        [Required]
        public int TeacherId { get; set; }
    }

    public class EnrollStudentDto
    {
        [Required]
        public int StudentId { get; set; }

        [Required]
        public int CourseId { get; set; }
    }

    public class CourseSubjectDto
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public int SubjectId { get; set; }
        public string SubjectName { get; set; } = string.Empty;
        public int TeacherId { get; set; }
        public string TeacherName { get; set; } = string.Empty;
    }
}
