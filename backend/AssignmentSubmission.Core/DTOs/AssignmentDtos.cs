/*
 * File: AssignmentDtos.cs
 * Purpose: Data Transfer Objects for creating, updating, and displaying assignment details.
 * 
 * Dependencies Used:
 * - Enums.cs: AssignmentStatus enum.
 * 
 * Used By:
 * - TeacherController.cs & StudentController.cs: API Request/Response payload models.
 * - AssignmentService.cs: Business logic data containers.
 */

using System;
using System.ComponentModel.DataAnnotations;
using AssignmentSubmission.Core.Entities;

namespace AssignmentSubmission.Core.DTOs
{
    public class CreateAssignmentDto
    {
        [Required]
        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        [Range(1, 1000)]
        public int MaxMarks { get; set; }

        [Required]
        public DateTime Deadline { get; set; }

        [Required]
        public int CourseSubjectId { get; set; }

        public bool IsPublished { get; set; } = false;
    }

    public class UpdateAssignmentDto
    {
        [Required]
        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        [Range(1, 1000)]
        public int MaxMarks { get; set; }

        [Required]
        public DateTime Deadline { get; set; }

        public bool IsPublished { get; set; }
    }

    public class AssignmentDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int MaxMarks { get; set; }
        public DateTime Deadline { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        
        public int CourseSubjectId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public string SubjectName { get; set; } = string.Empty;
        public string TeacherName { get; set; } = string.Empty;
    }
}
