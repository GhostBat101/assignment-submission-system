/*
 * File: AdminController.cs
 * Purpose: Secured REST API Controller for Admin tasks (Managing users, courses, subjects, teacher assignments, student enrollments).
 * 
 * Dependencies Used:
 * - IAdminService.cs: Administrative operations business logic.
 * - AdminDtos.cs & AuthDtos.cs: Request/Response DTO models.
 * - Microsoft.AspNetCore.Authorization: Enforces `[Authorize(Roles = "Admin")]` security.
 * 
 * Used By:
 * - Next.js Admin Dashboard (`/admin/*` pages).
 * - Swagger/OpenAPI UI.
 */

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AssignmentSubmission.Core.DTOs;
using AssignmentSubmission.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssignmentSubmission.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;

        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _adminService.GetAllUsersAsync();
            return Ok(users);
        }

        [HttpPost("users")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDto dto)
        {
            try
            {
                var user = await _adminService.CreateUserAsync(dto);
                return CreatedAtAction(nameof(GetUsers), new { id = user.Id }, user);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("courses")]
        public async Task<IActionResult> GetCourses()
        {
            var courses = await _adminService.GetAllCoursesAsync();
            return Ok(courses);
        }

        [HttpPost("courses")]
        public async Task<IActionResult> CreateCourse([FromBody] CreateCourseDto dto)
        {
            var course = await _adminService.CreateCourseAsync(dto);
            return Ok(course);
        }

        [HttpGet("subjects")]
        public async Task<IActionResult> GetSubjects()
        {
            var subjects = await _adminService.GetAllSubjectsAsync();
            return Ok(subjects);
        }

        [HttpPost("subjects")]
        public async Task<IActionResult> CreateSubject([FromBody] CreateSubjectDto dto)
        {
            var subject = await _adminService.CreateSubjectAsync(dto);
            return Ok(subject);
        }

        [HttpGet("course-subjects")]
        public async Task<IActionResult> GetCourseSubjects()
        {
            var list = await _adminService.GetAllCourseSubjectsAsync();
            return Ok(list);
        }

        [HttpPost("assign-teacher")]
        public async Task<IActionResult> AssignTeacher([FromBody] AssignTeacherDto dto)
        {
            try
            {
                var result = await _adminService.AssignTeacherToSubjectAsync(dto);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPost("enroll-student")]
        public async Task<IActionResult> EnrollStudent([FromBody] EnrollStudentDto dto)
        {
            try
            {
                var result = await _adminService.EnrollStudentInCourseAsync(dto);
                return Ok(new { success = result, message = "Student successfully enrolled in course." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpGet("assignments")]
        public async Task<ActionResult<PaginatedResponse<AssignmentDto>>> GetAllAssignments([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
        {
            var assignments = await _adminService.GetAllAssignmentsAsync(page, pageSize);
            return Ok(assignments);
        }

        [HttpGet("submissions")]
        public async Task<ActionResult<PaginatedResponse<SubmissionDto>>> GetAllSubmissions([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
        {
            var submissions = await _adminService.GetAllSubmissionsAsync(page, pageSize);
            return Ok(submissions);
        }
    }
}
