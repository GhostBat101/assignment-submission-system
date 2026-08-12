/*
 * File: Subject.cs
 * Purpose: Represents an academic subject (e.g., "Mathematics", "Physics", "Computer Programming").
 * 
 * Dependencies Used:
 * - CourseSubject.cs: Junction entity linking subject to courses and assigned teachers.
 * 
 * Used By:
 * - ApplicationDbContext.cs: Database mapping.
 * - SubjectService.cs: Business logic for managing subjects.
 */

using System.Collections.Generic;

namespace AssignmentSubmission.Core.Entities
{
    public class Subject
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;

        // Navigation Properties
        public ICollection<CourseSubject> CourseSubjects { get; set; } = new List<CourseSubject>();
    }
}
