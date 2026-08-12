/*
 * File: Enums.cs
 * Purpose: Defines core enumeration types for User Roles, Assignment Statuses, and Submission Statuses.
 * 
 * Dependencies Used: None
 * 
 * Used By:
 * - User.cs, Assignment.cs, Submission.cs (Domain Entities)
 * - DTOs and Services across backend projects
 */

namespace AssignmentSubmission.Core.Entities
{
    public enum UserRole
    {
        Admin = 1,
        Teacher = 2,
        Student = 3
    }

    public enum AssignmentStatus
    {
        Draft = 1,
        Published = 2
    }

    public enum SubmissionStatus
    {
        Submitted = 1,
        Graded = 2,
        Resubmitted = 3
    }
}
