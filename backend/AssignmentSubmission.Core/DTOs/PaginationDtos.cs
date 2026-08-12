/*
 * File: PaginationDtos.cs
 * Purpose: Provides a generic wrapper for paginated API responses to ensure scalable data retrieval.
 * 
 * Dependencies Used: None
 * 
 * Used By:
 * - AdminService.cs, TeacherService.cs, StudentService.cs
 * - Controllers returning lists of entities (Assignments, Submissions).
 */

using System;
using System.Collections.Generic;

namespace AssignmentSubmission.Core.DTOs
{
    public class PaginatedResponse<T>
    {
        public IEnumerable<T> Data { get; set; } = new List<T>();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
        public bool HasNextPage => Page < TotalPages;
        public bool HasPreviousPage => Page > 1;

        public PaginatedResponse(IEnumerable<T> data, int count, int page, int pageSize)
        {
            Data = data;
            TotalCount = count;
            Page = page;
            PageSize = pageSize;
        }
    }
}
