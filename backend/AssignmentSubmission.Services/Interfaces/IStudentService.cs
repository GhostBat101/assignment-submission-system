/*
 * File: IStudentService.cs
 * Purpose: Interface contract for Student operations (Viewing enrolled assignments, submitting work, updating work pre-deadline, checking feedback).
 * 
 * Dependencies Used:
 * - AssignmentDtos.cs & SubmissionDtos.cs
 * 
 * Used By:
 * - StudentService.cs: Implementation class.
 * - StudentController.cs: Injected into HTTP endpoints.
 */

using System.Collections.Generic;
using System.Threading.Tasks;
using AssignmentSubmission.Core.DTOs;

namespace AssignmentSubmission.Services.Interfaces
{
    public interface IStudentService
    {
        Task<IEnumerable<AssignmentDto>> GetMyAssignmentsAsync(int studentId);
        Task<AssignmentDto> GetAssignmentDetailsAsync(int studentId, int assignmentId);
        Task<SubmissionDto> SubmitAssignmentAsync(int studentId, CreateSubmissionDto dto);
        Task<SubmissionDto> UpdateSubmissionAsync(int studentId, int submissionId, CreateSubmissionDto dto);
        Task<IEnumerable<SubmissionDto>> GetMySubmissionsAsync(int studentId);
        Task<SubmissionDto?> GetSubmissionForAssignmentAsync(int studentId, int assignmentId);
    }
}
