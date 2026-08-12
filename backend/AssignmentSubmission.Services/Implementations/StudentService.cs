/*
 * File: StudentService.cs
 * Purpose: Business logic service for Student actions (viewing published assignments, submitting answers, pre-deadline updates, checking grades).
 * 
 * Dependencies Used:
 * - ApplicationDbContext.cs: Database querying.
 * - IStudentService.cs: Interface.
 * - AssignmentDtos.cs & SubmissionDtos.cs: DTO contracts.
 * - Entities (Submission, Assignment, StudentCourse, Enums)
 * 
 * Used By:
 * - StudentController.cs: Handles Student API endpoints.
 * - Program.cs: Dependency Injection registration.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AssignmentSubmission.Core.DTOs;
using AssignmentSubmission.Core.Entities;
using AssignmentSubmission.Infrastructure.Data;
using AssignmentSubmission.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AssignmentSubmission.Services.Implementations
{
    public class StudentService : IStudentService
    {
        private readonly ApplicationDbContext _context;

        public StudentService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<AssignmentDto>> GetMyAssignmentsAsync(int studentId)
        {
            // Get courses the student is enrolled in
            var courseIds = await _context.StudentCourses
                .Where(sc => sc.StudentId == studentId)
                .Select(sc => sc.CourseId)
                .ToListAsync();

            return await _context.Assignments
                .Include(a => a.CourseSubject).ThenInclude(cs => cs.Course)
                .Include(a => a.CourseSubject).ThenInclude(cs => cs.Subject)
                .Include(a => a.CourseSubject).ThenInclude(cs => cs.Teacher)
                .Where(a => courseIds.Contains(a.CourseSubject.CourseId) && a.Status == AssignmentStatus.Published)
                .Select(a => MapToAssignmentDto(a))
                .ToListAsync();
        }

        public async Task<AssignmentDto> GetAssignmentDetailsAsync(int studentId, int assignmentId)
        {
            var assignment = await _context.Assignments
                .Include(a => a.CourseSubject).ThenInclude(cs => cs.Course)
                .Include(a => a.CourseSubject).ThenInclude(cs => cs.Subject)
                .Include(a => a.CourseSubject).ThenInclude(cs => cs.Teacher)
                .FirstOrDefaultAsync(a => a.Id == assignmentId && a.Status == AssignmentStatus.Published);

            if (assignment == null)
            {
                throw new KeyNotFoundException("Assignment not found or is not published.");
            }

            var isEnrolled = await _context.StudentCourses
                .AnyAsync(sc => sc.StudentId == studentId && sc.CourseId == assignment.CourseSubject.CourseId);

            if (!isEnrolled)
            {
                throw new UnauthorizedAccessException("You are not enrolled in the course for this assignment.");
            }

            return MapToAssignmentDto(assignment);
        }

        public async Task<SubmissionDto> SubmitAssignmentAsync(int studentId, CreateSubmissionDto dto)
        {
            var assignment = await _context.Assignments
                .Include(a => a.CourseSubject)
                .FirstOrDefaultAsync(a => a.Id == dto.AssignmentId && a.Status == AssignmentStatus.Published);

            if (assignment == null)
            {
                throw new KeyNotFoundException("Assignment not found or is not published.");
            }

            var isEnrolled = await _context.StudentCourses
                .AnyAsync(sc => sc.StudentId == studentId && sc.CourseId == assignment.CourseSubject.CourseId);

            if (!isEnrolled)
            {
                throw new UnauthorizedAccessException("You are not enrolled in this course.");
            }

            // CRITICAL BUSINESS RULE: Deadline Enforcement
            if (DateTime.UtcNow > assignment.Deadline)
            {
                throw new InvalidOperationException("Cannot submit answer. The assignment deadline has already passed.");
            }

            var existingSubmission = await _context.Submissions
                .FirstOrDefaultAsync(s => s.StudentId == studentId && s.AssignmentId == dto.AssignmentId);

            if (existingSubmission != null)
            {
                throw new InvalidOperationException("You have already submitted an answer for this assignment. Please use the update option instead.");
            }

            var submission = new Submission
            {
                AssignmentId = dto.AssignmentId,
                StudentId = studentId,
                AnswerText = dto.AnswerText,
                AttachmentUrl = dto.AttachmentUrl,
                SubmittedAt = DateTime.UtcNow,
                Status = SubmissionStatus.Submitted
            };

            _context.Submissions.Add(submission);
            await _context.SaveChangesAsync();

            return await GetSubmissionByIdAsync(submission.Id);
        }

        public async Task<SubmissionDto> UpdateSubmissionAsync(int studentId, int submissionId, CreateSubmissionDto dto)
        {
            var submission = await _context.Submissions
                .Include(s => s.Assignment)
                .FirstOrDefaultAsync(s => s.Id == submissionId);

            if (submission == null) throw new KeyNotFoundException("Submission not found.");
            if (submission.StudentId != studentId)
            {
                throw new UnauthorizedAccessException("You are not authorized to edit this submission.");
            }

            // CRITICAL BUSINESS RULE: Pre-deadline update restriction
            if (DateTime.UtcNow > submission.Assignment.Deadline)
            {
                throw new InvalidOperationException("Cannot update submission. The assignment deadline has already passed.");
            }

            submission.AnswerText = dto.AnswerText;
            submission.AttachmentUrl = dto.AttachmentUrl;
            submission.UpdatedAt = DateTime.UtcNow;
            submission.Status = SubmissionStatus.Resubmitted;

            await _context.SaveChangesAsync();
            return await GetSubmissionByIdAsync(submission.Id);
        }

        public async Task<IEnumerable<SubmissionDto>> GetMySubmissionsAsync(int studentId)
        {
            return await _context.Submissions
                .Include(s => s.Student)
                .Include(s => s.Assignment)
                .Where(s => s.StudentId == studentId)
                .Select(s => MapToSubmissionDto(s))
                .ToListAsync();
        }

        public async Task<SubmissionDto?> GetSubmissionForAssignmentAsync(int studentId, int assignmentId)
        {
            var s = await _context.Submissions
                .Include(sub => sub.Student)
                .Include(sub => sub.Assignment)
                .FirstOrDefaultAsync(sub => sub.StudentId == studentId && sub.AssignmentId == assignmentId);

            return s == null ? null : MapToSubmissionDto(s);
        }

        private async Task<SubmissionDto> GetSubmissionByIdAsync(int id)
        {
            var s = await _context.Submissions
                .Include(sub => sub.Student)
                .Include(sub => sub.Assignment)
                .FirstAsync(sub => sub.Id == id);

            return MapToSubmissionDto(s);
        }

        private static AssignmentDto MapToAssignmentDto(Assignment a)
        {
            return new AssignmentDto
            {
                Id = a.Id,
                Title = a.Title,
                Description = a.Description,
                MaxMarks = a.MaxMarks,
                Deadline = a.Deadline,
                Status = a.Status.ToString(),
                CreatedAt = a.CreatedAt,
                CourseSubjectId = a.CourseSubjectId,
                CourseName = a.CourseSubject.Course.Name,
                SubjectName = a.CourseSubject.Subject.Name,
                TeacherName = a.CourseSubject.Teacher.FullName
            };
        }

        private static SubmissionDto MapToSubmissionDto(Submission s)
        {
            return new SubmissionDto
            {
                Id = s.Id,
                AssignmentId = s.AssignmentId,
                AssignmentTitle = s.Assignment.Title,
                MaxMarks = s.Assignment.MaxMarks,
                Deadline = s.Assignment.Deadline,
                StudentId = s.StudentId,
                StudentName = s.Student.FullName,
                StudentEmail = s.Student.Email,
                AnswerText = s.AnswerText,
                AttachmentUrl = s.AttachmentUrl,
                SubmittedAt = s.SubmittedAt,
                UpdatedAt = s.UpdatedAt,
                Status = s.Status.ToString(),
                MarksAwarded = s.MarksAwarded,
                Feedback = s.Feedback,
                GradedAt = s.GradedAt
            };
        }
    }
}
