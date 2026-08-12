/*
 * File: Course.cs
 * Purpose: Represents a class or course cohort (e.g., "Grade 10", "Computer Science 101").
 * 
 * Dependencies Used:
 * - CourseSubject.cs: Junction entity linking course to subjects.
 * - StudentCourse.cs: Junction entity linking course to enrolled students.
 * 
 * Used By:
 * - ApplicationDbContext.cs: Database mapping.
 * - CourseService.cs: Business logic for class/course management.
 */

using System.Collections.Generic;

namespace AssignmentSubmission.Core.Entities
{
    public class Course
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        // Navigation Properties
        public ICollection<CourseSubject> CourseSubjects { get; set; } = new List<CourseSubject>();
        public ICollection<StudentCourse> StudentCourses { get; set; } = new List<StudentCourse>();
    }
}
