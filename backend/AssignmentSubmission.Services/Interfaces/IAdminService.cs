/*
 * File: IAdminService.cs
 * Purpose: Interface contract for administrative operations (managing users, courses, subjects, assignments, and enrollments).
 * 
 * Dependencies Used:
 * - AdminDtos.cs & AuthDtos.cs
 * - Course.cs & Subject.cs
 * 
 * Used By:
 * - AdminService.cs: Implements these operations.
 * - AdminController.cs: Injects interface into HTTP endpoints.
 */

using System.Collections.Generic;
using System.Threading.Tasks;
using AssignmentSubmission.Core.DTOs;
using AssignmentSubmission.Core.Entities;

namespace AssignmentSubmission.Services.Interfaces
{
    public interface IAdminService
    {
        Task<IEnumerable<UserDto>> GetAllUsersAsync();
        Task<UserDto> CreateUserAsync(CreateUserDto dto);
        Task<IEnumerable<Course>> GetAllCoursesAsync();
        Task<Course> CreateCourseAsync(CreateCourseDto dto);
        Task<IEnumerable<Subject>> GetAllSubjectsAsync();
        Task<Subject> CreateSubjectAsync(CreateSubjectDto dto);
        Task<CourseSubjectDto> AssignTeacherToSubjectAsync(AssignTeacherDto dto);
        Task<bool> EnrollStudentInCourseAsync(EnrollStudentDto dto);
        Task<IEnumerable<CourseSubjectDto>> GetAllCourseSubjectsAsync();
        Task<PaginatedResponse<AssignmentDto>> GetAllAssignmentsAsync(int page = 1, int pageSize = 50);
        Task<PaginatedResponse<SubmissionDto>> GetAllSubmissionsAsync(int page = 1, int pageSize = 50);
    }
}
