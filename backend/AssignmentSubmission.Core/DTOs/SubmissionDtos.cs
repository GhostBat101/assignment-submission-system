/*
 * File: SubmissionDtos.cs
 * Purpose: Data Transfer Objects for student answer submission, teacher grading, and feedback retrieval.
 * 
 * Dependencies Used:
 * - System.ComponentModel.DataAnnotations: Validation attributes.
 * 
 * Used By:
 * - StudentController.cs: Accepts student answer submissions.
 * - TeacherController.cs: Accepts teacher grades and feedback.
 * - SubmissionService.cs: Core business logic payload processing.
 */

using System;
using System.ComponentModel.DataAnnotations;

namespace AssignmentSubmission.Core.DTOs
{
    public class CreateSubmissionDto
    {
        [Required]
        public int AssignmentId { get; set; }

        [Required]
        public string AnswerText { get; set; } = string.Empty;

        public string? AttachmentUrl { get; set; }
    }

    public class GradeSubmissionDto
    {
        [Required]
        [Range(0, 1000)]
        public int Marks { get; set; }

        public string? Feedback { get; set; }
    }

    public class SubmissionDto
    {
        public int Id { get; set; }
        public int AssignmentId { get; set; }
        public string AssignmentTitle { get; set; } = string.Empty;
        public int MaxMarks { get; set; }
        public DateTime Deadline { get; set; }
        
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string StudentEmail { get; set; } = string.Empty;

        public string AnswerText { get; set; } = string.Empty;
        public string? AttachmentUrl { get; set; }
        public DateTime SubmittedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public string Status { get; set; } = string.Empty;
        public int? MarksAwarded { get; set; }
        public string? Feedback { get; set; }
        public DateTime? GradedAt { get; set; }
    }
}
