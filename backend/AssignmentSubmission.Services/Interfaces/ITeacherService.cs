/*
 * File: ITeacherService.cs
 * Purpose: Interface contract for Teacher operations (Managing assignments, reviewing student submissions, grading).
 * 
 * Dependencies Used:
 * - AssignmentDtos.cs & SubmissionDtos.cs
 * - AdminDtos.cs (CourseSubjectDto)
 * 
 * Used By:
 * - TeacherService.cs: Implementation class.
 * - TeacherController.cs: Injected into HTTP endpoint handlers.
 */

using System.Collections.Generic;
using System.Threading.Tasks;
using AssignmentSubmission.Core.DTOs;

namespace AssignmentSubmission.Services.Interfaces
{
    public interface ITeacherService
    {
        Task<IEnumerable<CourseSubjectDto>> GetMyAssignedSubjectsAsync(int teacherId);
        Task<IEnumerable<AssignmentDto>> GetMyAssignmentsAsync(int teacherId);
        Task<AssignmentDto> CreateAssignmentAsync(int teacherId, CreateAssignmentDto dto);
        Task<AssignmentDto> UpdateAssignmentAsync(int teacherId, int assignmentId, UpdateAssignmentDto dto);
        Task<bool> DeleteAssignmentAsync(int teacherId, int assignmentId);
        Task<AssignmentDto> TogglePublishStatusAsync(int teacherId, int assignmentId, bool publish);
        
        Task<IEnumerable<SubmissionDto>> GetAssignmentSubmissionsAsync(int teacherId, int assignmentId);
        Task<SubmissionDto> GradeSubmissionAsync(int teacherId, int submissionId, GradeSubmissionDto dto);
    }
}
