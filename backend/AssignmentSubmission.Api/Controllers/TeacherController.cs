/*
 * File: TeacherController.cs
 * Purpose: Secured REST API Controller for Teacher operations (CRUD assignments, draft/publish toggling, submission review, grading).
 * 
 * Dependencies Used:
 * - ITeacherService.cs: Teacher operations service interface.
 * - AssignmentDtos.cs & SubmissionDtos.cs: Request/Response DTO models.
 * - System.Security.Claims: Extracts teacher user ID from JWT claims.
 * - Microsoft.AspNetCore.Authorization: Enforces `[Authorize(Roles = "Teacher")]`.
 * 
 * Used By:
 * - Next.js Teacher Dashboard (`/teacher/*` pages).
 * - Swagger/OpenAPI UI.
 */

using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using AssignmentSubmission.Core.DTOs;
using AssignmentSubmission.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssignmentSubmission.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Teacher")]
    public class TeacherController : ControllerBase
    {
        private readonly ITeacherService _teacherService;

        public TeacherController(ITeacherService teacherService)
        {
            _teacherService = teacherService;
        }

        private int GetCurrentUserId()
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
            {
                throw new UnauthorizedAccessException("User identity claim missing or invalid.");
            }
            return userId;
        }

        [HttpGet("subjects")]
        public async Task<IActionResult> GetMyAssignedSubjects()
        {
            var teacherId = GetCurrentUserId();
            var subjects = await _teacherService.GetMyAssignedSubjectsAsync(teacherId);
            return Ok(subjects);
        }

        [HttpGet("assignments")]
        public async Task<IActionResult> GetMyAssignments()
        {
            var teacherId = GetCurrentUserId();
            var assignments = await _teacherService.GetMyAssignmentsAsync(teacherId);
            return Ok(assignments);
        }

        [HttpPost("assignments")]
        public async Task<IActionResult> CreateAssignment([FromBody] CreateAssignmentDto dto)
        {
            try
            {
                var teacherId = GetCurrentUserId();
                var result = await _teacherService.CreateAssignmentAsync(teacherId, dto);
                return CreatedAtAction(nameof(GetMyAssignments), new { id = result.Id }, result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
        }

        [HttpPut("assignments/{id}")]
        public async Task<IActionResult> UpdateAssignment(int id, [FromBody] UpdateAssignmentDto dto)
        {
            try
            {
                var teacherId = GetCurrentUserId();
                var result = await _teacherService.UpdateAssignmentAsync(teacherId, id, dto);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
        }

        [HttpDelete("assignments/{id}")]
        public async Task<IActionResult> DeleteAssignment(int id)
        {
            try
            {
                var teacherId = GetCurrentUserId();
                await _teacherService.DeleteAssignmentAsync(teacherId, id);
                return Ok(new { message = "Assignment deleted successfully." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
        }

        [HttpPatch("assignments/{id}/publish")]
        public async Task<IActionResult> TogglePublish(int id, [FromQuery] bool publish = true)
        {
            try
            {
                var teacherId = GetCurrentUserId();
                var result = await _teacherService.TogglePublishStatusAsync(teacherId, id, publish);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
        }

        [HttpGet("assignments/{assignmentId}/submissions")]
        public async Task<IActionResult> GetSubmissions(int assignmentId)
        {
            try
            {
                var teacherId = GetCurrentUserId();
                var list = await _teacherService.GetAssignmentSubmissionsAsync(teacherId, assignmentId);
                return Ok(list);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
        }

        [HttpPost("submissions/{submissionId}/grade")]
        public async Task<IActionResult> GradeSubmission(int submissionId, [FromBody] GradeSubmissionDto dto)
        {
            try
            {
                var teacherId = GetCurrentUserId();
                var result = await _teacherService.GradeSubmissionAsync(teacherId, submissionId, dto);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
