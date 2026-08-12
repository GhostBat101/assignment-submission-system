/*
 * File: Assignment.cs
 * Purpose: Represents an assignment created by a teacher for a specific course subject.
 * 
 * Dependencies Used:
 * - Enums.cs: For AssignmentStatus (Draft, Published).
 * - CourseSubject.cs: Entity linking assignment to a course, subject, and teacher.
 * - Submission.cs: Navigation property for all student submissions for this assignment.
 * 
 * Used By:
 * - ApplicationDbContext.cs: Database entity mapping.
 * - AssignmentService.cs: Business logic for assignment creation, editing, and publishing.
 */

using System;
using System.Collections.Generic;

namespace AssignmentSubmission.Core.Entities
{
    public class Assignment
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int MaxMarks { get; set; }
        public DateTime Deadline { get; set; }
        public AssignmentStatus Status { get; set; } = AssignmentStatus.Draft;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Foreign Key
        public int CourseSubjectId { get; set; }
        public CourseSubject CourseSubject { get; set; } = null!;

        // Navigation Properties
        public ICollection<Submission> Submissions { get; set; } = new List<Submission>();
    }
}
