/*
 * File: TeacherService.cs
 * Purpose: Implements business logic for Teachers (creating/editing/deleting assignments, draft management, submission grading).
 * 
 * Dependencies Used:
 * - ApplicationDbContext.cs: Database access.
 * - ITeacherService.cs: Interface definition.
 * - AssignmentDtos.cs, SubmissionDtos.cs, AdminDtos.cs: DTO models.
 * 
 * Used By:
 * - TeacherController.cs: Fulfills Teacher endpoints.
 * - Program.cs: Registered in Dependency Injection.
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
    public class TeacherService : ITeacherService
    {
        private readonly ApplicationDbContext _context;

        public TeacherService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CourseSubjectDto>> GetMyAssignedSubjectsAsync(int teacherId)
        {
            return await _context.CourseSubjects
                .Include(cs => cs.Course)
                .Include(cs => cs.Subject)
                .Where(cs => cs.TeacherId == teacherId)
                .Select(cs => new CourseSubjectDto
                {
                    Id = cs.Id,
                    CourseId = cs.CourseId,
                    CourseName = cs.Course.Name,
                    SubjectId = cs.SubjectId,
                    SubjectName = cs.Subject.Name,
                    TeacherId = cs.TeacherId,
                    TeacherName = cs.Teacher.FullName
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<AssignmentDto>> GetMyAssignmentsAsync(int teacherId)
        {
            return await _context.Assignments
                .Include(a => a.CourseSubject)
                    .ThenInclude(cs => cs.Course)
                .Include(a => a.CourseSubject)
                    .ThenInclude(cs => cs.Subject)
                .Include(a => a.CourseSubject)
                    .ThenInclude(cs => cs.Teacher)
                .Where(a => a.CourseSubject.TeacherId == teacherId)
                .Select(a => MapToAssignmentDto(a))
                .ToListAsync();
        }

        public async Task<AssignmentDto> CreateAssignmentAsync(int teacherId, CreateAssignmentDto dto)
        {
            var cs = await _context.CourseSubjects
                .FirstOrDefaultAsync(x => x.Id == dto.CourseSubjectId && x.TeacherId == teacherId);

            if (cs == null)
            {
                throw new UnauthorizedAccessException("Teacher is not assigned to this course subject.");
            }

            var assignment = new Assignment
            {
                Title = dto.Title,
                Description = dto.Description,
                MaxMarks = dto.MaxMarks,
                Deadline = dto.Deadline,
                Status = dto.IsPublished ? AssignmentStatus.Published : AssignmentStatus.Draft,
                CourseSubjectId = dto.CourseSubjectId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Assignments.Add(assignment);
            await _context.SaveChangesAsync();

            return await GetAssignmentByIdAsync(assignment.Id);
        }

        public async Task<AssignmentDto> UpdateAssignmentAsync(int teacherId, int assignmentId, UpdateAssignmentDto dto)
        {
            var assignment = await _context.Assignments
                .Include(a => a.CourseSubject)
                .FirstOrDefaultAsync(a => a.Id == assignmentId);

            if (assignment == null) throw new KeyNotFoundException("Assignment not found.");
            if (assignment.CourseSubject.TeacherId != teacherId)
            {
                throw new UnauthorizedAccessException("You are not authorized to edit this assignment.");
            }

            assignment.Title = dto.Title;
            assignment.Description = dto.Description;
            assignment.MaxMarks = dto.MaxMarks;
            assignment.Deadline = dto.Deadline;
            assignment.Status = dto.IsPublished ? AssignmentStatus.Published : AssignmentStatus.Draft;

            await _context.SaveChangesAsync();
            return await GetAssignmentByIdAsync(assignment.Id);
        }

        public async Task<bool> DeleteAssignmentAsync(int teacherId, int assignmentId)
        {
            var assignment = await _context.Assignments
                .Include(a => a.CourseSubject)
                .FirstOrDefaultAsync(a => a.Id == assignmentId);

            if (assignment == null) throw new KeyNotFoundException("Assignment not found.");
            if (assignment.CourseSubject.TeacherId != teacherId)
            {
                throw new UnauthorizedAccessException("You are not authorized to delete this assignment.");
            }

            _context.Assignments.Remove(assignment);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<AssignmentDto> TogglePublishStatusAsync(int teacherId, int assignmentId, bool publish)
        {
            var assignment = await _context.Assignments
                .Include(a => a.CourseSubject)
                .FirstOrDefaultAsync(a => a.Id == assignmentId);

            if (assignment == null) throw new KeyNotFoundException("Assignment not found.");
            if (assignment.CourseSubject.TeacherId != teacherId)
            {
                throw new UnauthorizedAccessException("You are not authorized to update this assignment.");
            }

            assignment.Status = publish ? AssignmentStatus.Published : AssignmentStatus.Draft;
            await _context.SaveChangesAsync();

            return await GetAssignmentByIdAsync(assignment.Id);
        }

        public async Task<IEnumerable<SubmissionDto>> GetAssignmentSubmissionsAsync(int teacherId, int assignmentId)
        {
            var assignment = await _context.Assignments
                .Include(a => a.CourseSubject)
                .FirstOrDefaultAsync(a => a.Id == assignmentId);

            if (assignment == null) throw new KeyNotFoundException("Assignment not found.");
            if (assignment.CourseSubject.TeacherId != teacherId)
            {
                throw new UnauthorizedAccessException("You are not authorized to view submissions for this assignment.");
            }

            return await _context.Submissions
                .Include(s => s.Student)
                .Include(s => s.Assignment)
                .Where(s => s.AssignmentId == assignmentId)
                .Select(s => MapToSubmissionDto(s))
                .ToListAsync();
        }

        public async Task<SubmissionDto> GradeSubmissionAsync(int teacherId, int submissionId, GradeSubmissionDto dto)
        {
            var submission = await _context.Submissions
                .Include(s => s.Assignment)
                    .ThenInclude(a => a.CourseSubject)
                .Include(s => s.Student)
                .FirstOrDefaultAsync(s => s.Id == submissionId);

            if (submission == null) throw new KeyNotFoundException("Submission not found.");
            if (submission.Assignment.CourseSubject.TeacherId != teacherId)
            {
                throw new UnauthorizedAccessException("You are not authorized to grade this submission.");
            }

            if (dto.Marks < 0)
            {
                throw new ArgumentException("Marks cannot be negative.");
            }

            if (dto.Marks > submission.Assignment.MaxMarks)
            {
                throw new ArgumentException($"Marks cannot exceed maximum assignment marks ({submission.Assignment.MaxMarks}).");
            }

            submission.MarksAwarded = dto.Marks;
            submission.Feedback = dto.Feedback;
            submission.Status = SubmissionStatus.Graded;
            submission.GradedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return MapToSubmissionDto(submission);
        }

        private async Task<AssignmentDto> GetAssignmentByIdAsync(int id)
        {
            var a = await _context.Assignments
                .Include(x => x.CourseSubject).ThenInclude(cs => cs.Course)
                .Include(x => x.CourseSubject).ThenInclude(cs => cs.Subject)
                .Include(x => x.CourseSubject).ThenInclude(cs => cs.Teacher)
                .FirstAsync(x => x.Id == id);

            return MapToAssignmentDto(a);
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
