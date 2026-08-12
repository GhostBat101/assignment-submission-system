/*
 * File: CourseSubject.cs
 * Purpose: Represents the assignment of a Subject to a Course, along with the assigned Teacher.
 * 
 * Dependencies Used:
 * - Course.cs: The course/class entity.
 * - Subject.cs: The subject entity.
 * - User.cs: The teacher entity assigned to teach this course subject.
 * - Assignment.cs: List of assignments created for this subject in this course.
 * 
 * Used By:
 * - ApplicationDbContext.cs: Database mapping.
 * - Services: Verification of teacher authorization to create assignments.
 */

using System.Collections.Generic;

namespace AssignmentSubmission.Core.Entities
{
    public class CourseSubject
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public Course Course { get; set; } = null!;

        public int SubjectId { get; set; }
        public Subject Subject { get; set; } = null!;

        public int TeacherId { get; set; }
        public User Teacher { get; set; } = null!;

        // Navigation Properties
        public ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();
    }
}
