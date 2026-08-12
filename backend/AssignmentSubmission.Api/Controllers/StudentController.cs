/*
 * File: StudentController.cs
 * Purpose: Secured REST API Controller for Student actions (viewing assigned work, submitting answers, pre-deadline updates, checking grades).
 * 
 * Dependencies Used:
 * - IStudentService.cs: Student operations service interface.
 * - AssignmentDtos.cs & SubmissionDtos.cs: DTO models.
 * - System.Security.Claims: Extracts student user ID from JWT claims.
 * - Microsoft.AspNetCore.Authorization: Enforces `[Authorize(Roles = "Student")]`.
 * 
 * Used By:
 * - Next.js Student Dashboard (`/student/*` pages).
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
    [Authorize(Roles = "Student")]
    public class StudentController : ControllerBase
    {
        private readonly IStudentService _studentService;

        public StudentController(IStudentService studentService)
        {
            _studentService = studentService;
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

        [HttpGet("assignments")]
        public async Task<IActionResult> GetMyAssignments()
        {
            var studentId = GetCurrentUserId();
            var list = await _studentService.GetMyAssignmentsAsync(studentId);
            return Ok(list);
        }

        [HttpGet("assignments/{id}")]
        public async Task<IActionResult> GetAssignmentDetails(int id)
        {
            try
            {
                var studentId = GetCurrentUserId();
                var assignment = await _studentService.GetAssignmentDetailsAsync(studentId, id);
                return Ok(assignment);
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

        [HttpPost("submissions")]
        public async Task<IActionResult> SubmitAssignment([FromBody] CreateSubmissionDto dto)
        {
            try
            {
                var studentId = GetCurrentUserId();
                var result = await _studentService.SubmitAssignmentAsync(studentId, dto);
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
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("submissions/{id}")]
        public async Task<IActionResult> UpdateSubmission(int id, [FromBody] CreateSubmissionDto dto)
        {
            try
            {
                var studentId = GetCurrentUserId();
                var result = await _studentService.UpdateSubmissionAsync(studentId, id, dto);
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
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("submissions")]
        public async Task<IActionResult> GetMySubmissions()
        {
            var studentId = GetCurrentUserId();
            var list = await _studentService.GetMySubmissionsAsync(studentId);
            return Ok(list);
        }

        [HttpGet("assignments/{assignmentId}/submission")]
        public async Task<IActionResult> GetSubmissionForAssignment(int assignmentId)
        {
            var studentId = GetCurrentUserId();
            var submission = await _studentService.GetSubmissionForAssignmentAsync(studentId, assignmentId);
            if (submission == null) return NotFound(new { message = "No submission found for this assignment." });
            return Ok(submission);
        }
    }
}
