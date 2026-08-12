/*
 * File: StudentCourse.cs
 * Purpose: Junction entity representing a Student's enrollment in a Course.
 * 
 * Dependencies Used:
 * - User.cs: The student entity.
 * - Course.cs: The course entity.
 * 
 * Used By:
 * - ApplicationDbContext.cs: Database mapping.
 * - Services: Verifying student course enrollment and assignment access.
 */

using System;

namespace AssignmentSubmission.Core.Entities
{
    public class StudentCourse
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public User Student { get; set; } = null!;

        public int CourseId { get; set; }
        public Course Course { get; set; } = null!;

        public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;
    }
}
