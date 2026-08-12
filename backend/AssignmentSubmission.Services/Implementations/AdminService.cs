/*
 * File: AdminService.cs
 * Purpose: Business logic service for Admin activities (User Management, Course/Subject Creation, Teacher Assignments, Student Enrollments).
 * 
 * Dependencies Used:
 * - ApplicationDbContext.cs: EF Core database context.
 * - PasswordHasher.cs: Hashes user passwords during user creation.
 * - IAdminService.cs: Interface definition.
 * - AdminDtos.cs & AuthDtos.cs: Data transfer models.
 * 
 * Used By:
 * - AdminController.cs: Fulfills Admin HTTP API endpoints.
 * - Program.cs: Registered in DI container.
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
    public class AdminService : IAdminService
    {
        private readonly ApplicationDbContext _context;

        public AdminService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
        {
            return await _context.Users
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    Email = u.Email,
                    Role = u.Role.ToString()
                })
                .ToListAsync();
        }

        public async Task<UserDto> CreateUserAsync(CreateUserDto dto)
        {
            var existingUser = await _context.Users.AnyAsync(u => u.Email.ToLower() == dto.Email.ToLower());
            if (existingUser)
            {
                throw new InvalidOperationException("A user with this email already exists.");
            }

            var user = new User
            {
                FullName = dto.FullName,
                Email = dto.Email,
                PasswordHash = PasswordHasher.HashPassword(dto.Password),
                Role = dto.Role,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return new UserDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role.ToString()
            };
        }

        public async Task<IEnumerable<Course>> GetAllCoursesAsync()
        {
            return await _context.Courses.ToListAsync();
        }

        public async Task<Course> CreateCourseAsync(CreateCourseDto dto)
        {
            var course = new Course
            {
                Name = dto.Name,
                Description = dto.Description
            };

            _context.Courses.Add(course);
            await _context.SaveChangesAsync();
            return course;
        }

        public async Task<IEnumerable<Subject>> GetAllSubjectsAsync()
        {
            return await _context.Subjects.ToListAsync();
        }

        public async Task<Subject> CreateSubjectAsync(CreateSubjectDto dto)
        {
            var subject = new Subject
            {
                Name = dto.Name,
                Code = dto.Code
            };

            _context.Subjects.Add(subject);
            await _context.SaveChangesAsync();
            return subject;
        }

        public async Task<CourseSubjectDto> AssignTeacherToSubjectAsync(AssignTeacherDto dto)
        {
            var teacher = await _context.Users.FirstOrDefaultAsync(u => u.Id == dto.TeacherId && u.Role == UserRole.Teacher);
            if (teacher == null) throw new KeyNotFoundException("Teacher not found or specified user is not a Teacher.");

            var course = await _context.Courses.FindAsync(dto.CourseId);
            if (course == null) throw new KeyNotFoundException("Course not found.");

            var subject = await _context.Subjects.FindAsync(dto.SubjectId);
            if (subject == null) throw new KeyNotFoundException("Subject not found.");

            var existing = await _context.CourseSubjects
                .FirstOrDefaultAsync(cs => cs.CourseId == dto.CourseId && cs.SubjectId == dto.SubjectId);

            if (existing != null)
            {
                existing.TeacherId = dto.TeacherId;
            }
            else
            {
                existing = new CourseSubject
                {
                    CourseId = dto.CourseId,
                    SubjectId = dto.SubjectId,
                    TeacherId = dto.TeacherId
                };
                _context.CourseSubjects.Add(existing);
            }

            await _context.SaveChangesAsync();

            return new CourseSubjectDto
            {
                Id = existing.Id,
                CourseId = course.Id,
                CourseName = course.Name,
                SubjectId = subject.Id,
                SubjectName = subject.Name,
                TeacherId = teacher.Id,
                TeacherName = teacher.FullName
            };
        }

        public async Task<bool> EnrollStudentInCourseAsync(EnrollStudentDto dto)
        {
            var student = await _context.Users.FirstOrDefaultAsync(u => u.Id == dto.StudentId && u.Role == UserRole.Student);
            if (student == null) throw new KeyNotFoundException("Student not found or specified user is not a Student.");

            var course = await _context.Courses.FindAsync(dto.CourseId);
            if (course == null) throw new KeyNotFoundException("Course not found.");

            var alreadyEnrolled = await _context.StudentCourses
                .AnyAsync(sc => sc.StudentId == dto.StudentId && sc.CourseId == dto.CourseId);

            if (alreadyEnrolled)
            {
                throw new InvalidOperationException("Student is already enrolled in this course.");
            }

            var enrollment = new StudentCourse
            {
                StudentId = dto.StudentId,
                CourseId = dto.CourseId,
                EnrolledAt = DateTime.UtcNow
            };

            _context.StudentCourses.Add(enrollment);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<CourseSubjectDto>> GetAllCourseSubjectsAsync()
        {
            return await _context.CourseSubjects
                .Include(cs => cs.Course)
                .Include(cs => cs.Subject)
                .Include(cs => cs.Teacher)
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
        public async Task<PaginatedResponse<AssignmentDto>> GetAllAssignmentsAsync(int page = 1, int pageSize = 50)
        {
            var query = _context.Assignments
                .Include(a => a.CourseSubject).ThenInclude(cs => cs.Course)
                .Include(a => a.CourseSubject).ThenInclude(cs => cs.Subject)
                .Include(a => a.CourseSubject).ThenInclude(cs => cs.Teacher)
                .OrderByDescending(a => a.CreatedAt);

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(a => new AssignmentDto
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
                })
                .ToListAsync();

            return new PaginatedResponse<AssignmentDto>(items, totalCount, page, pageSize);
        }

        public async Task<PaginatedResponse<SubmissionDto>> GetAllSubmissionsAsync(int page = 1, int pageSize = 50)
        {
            var query = _context.Submissions
                .Include(s => s.Assignment)
                .Include(s => s.Student)
                .OrderByDescending(s => s.SubmittedAt);

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(s => new SubmissionDto
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
                })
                .ToListAsync();

            return new PaginatedResponse<SubmissionDto>(items, totalCount, page, pageSize);
        }
    }
}
