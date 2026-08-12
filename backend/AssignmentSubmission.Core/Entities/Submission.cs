/*
 * File: Submission.cs
 * Purpose: Represents a student's answer submission for a given assignment, along with marks and teacher feedback.
 * 
 * Dependencies Used:
 * - Enums.cs: For SubmissionStatus (Submitted, Graded, Resubmitted).
 * - Assignment.cs: The assignment entity being submitted for.
 * - User.cs: The student entity who submitted the answer.
 * 
 * Used By:
 * - ApplicationDbContext.cs: Database mapping.
 * - SubmissionService.cs: Core logic for submitting work, updating submissions, and teacher evaluation/grading.
 */

using System;

namespace AssignmentSubmission.Core.Entities
{
    public class Submission
    {
        public int Id { get; set; }
        public string AnswerText { get; set; } = string.Empty;
        public string? AttachmentUrl { get; set; }
        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        
        public SubmissionStatus Status { get; set; } = SubmissionStatus.Submitted;
        
        // Grading & Feedback (filled by Teacher)
        public int? MarksAwarded { get; set; }
        public string? Feedback { get; set; }
        public DateTime? GradedAt { get; set; }

        // Foreign Keys
        public int AssignmentId { get; set; }
        public Assignment Assignment { get; set; } = null!;

        public int StudentId { get; set; }
        public User Student { get; set; } = null!;
    }
}
