/*
 * File: User.cs
 * Purpose: Represents a user (Admin, Teacher, or Student) in the system.
 * 
 * Dependencies Used:
 * - Enums.cs: For UserRole enumeration.
 * - StudentCourse.cs: Navigation property for student's course enrollments.
 * - CourseSubject.cs: Navigation property for teacher's subject assignments.
 * - Submission.cs: Navigation property for student's submissions.
 * 
 * Used By:
 * - ApplicationDbContext.cs: For database table mapping.
 * - Services & Repositories: For authentication and user management.
 */

using System;
using System.Collections.Generic;

namespace AssignmentSubmission.Core.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public UserRole Role { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public ICollection<StudentCourse> StudentCourses { get; set; } = new List<StudentCourse>();
        public ICollection<CourseSubject> TaughtSubjects { get; set; } = new List<CourseSubject>();
        public ICollection<Submission> Submissions { get; set; } = new List<Submission>();
    }
}
