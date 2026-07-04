using Asp.Versioning;
using LeaveManagement.API.Models;
using LeaveManagement.Application.Common;
using LeaveManagement.Application.DTOs;
using LeaveManagement.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LeaveManagement.API.Controllers.V1
{
    /// <summary>
    /// API Controller for managing leave requests, including submission, querying, approvals, rejections, and cancellations.
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Authorize]
    public class LeaveRequestController : ControllerBase
    {
        private readonly ILeaveRequestService _service;

        public LeaveRequestController(ILeaveRequestService service)
        {
            _service = service;
        }

        /// <summary>
        /// Submits a new leave request for the authenticated employee.
        /// </summary>
        /// <param name="dto">The details of the leave request to create.</param>
        /// <returns>The created leave request detail.</returns>
        /// <response code="201">Returned when the request is successfully created.</response>
        /// <response code="400">Returned if request validations fail.</response>
        /// <response code="401">Returned if the user is unauthenticated.</response>
        [HttpPost]
        public async Task<ActionResult<LeaveRequestDto>> SubmitLeaveRequest(CreateLeaveRequestDto dto) 
        {
            int employeeId = GetCurrentUserId();
            var result = await _service.SubmitLeaveRequestAsync(dto, employeeId);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        /// <summary>
        /// Retrieves a specific leave request by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the leave request.</param>
        /// <returns>The leave request detail.</returns>
        /// <response code="200">Returned with the requested leave request.</response>
        /// <response code="401">Returned if the user is unauthenticated.</response>
        /// <response code="403">Returned if the user is unauthorized to view this request.</response>
        /// <response code="404">Returned if no leave request matches the specified ID.</response>
        [HttpGet("{id}")]
        public async Task<ActionResult<LeaveRequestDto>> GetById(Guid id) 
        {
            var result = await _service.GetByIdAsync(id);

            if (result == null)
                return NotFound();

            return Ok(result);
        }

        /// <summary>
        /// Retrieves a paginated list of leave requests submitted by a specific employee.
        /// </summary>
        /// <param name="employeeId">The ID of the employee whose requests to fetch.</param>
        /// <param name="pagination">Pagination parameters (page, pageSize).</param>
        /// <param name="sortParams">Sorting options.</param>
        /// <param name="filterParams">Filter criteria.</param>
        /// <returns>A paginated list of leave request summaries.</returns>
        /// <response code="200">Returned with the list of matching leave request summaries.</response>
        /// <response code="400">Returned if the pagination options are invalid.</response>
        /// <response code="401">Returned if the user is unauthenticated.</response>
        [HttpGet("employee/{employeeId}")]
        public async Task<ActionResult<PagedResponse<LeaveRequestSummaryDto>>> GetByEmployeeId(int employeeId, 
            [FromQuery] LeaveRequestPaginationParams pagination,
            [FromQuery] LeaveRequestSortParams sortParams,
            [FromQuery] LeaveRequestFilterParams filterParams)
        {
            var result = await _service.GetByEmployeeIdAsync(employeeId, pagination, sortParams, filterParams);

            var response = new PagedResponse<LeaveRequestSummaryDto>()
            {
                Items = result.Item2,
                TotalCount = result.Item1,
                PageSize = pagination.PageSize,
                Page = pagination.Page,
            };

            return Ok(response);
        }

        /// <summary>
        /// Retrieves a paginated list of all leave requests across the system.
        /// </summary>
        /// <param name="pagination">Pagination parameters.</param>
        /// <param name="sortParams">Sorting options.</param>
        /// <param name="filterParams">Filter criteria.</param>
        /// <returns>A paginated list of leave request summaries.</returns>
        /// <response code="200">Returned with the list of all matching leave request summaries.</response>
        /// <response code="400">Returned if pagination options are invalid.</response>
        /// <response code="401">Returned if the user is unauthenticated.</response>
        /// <response code="403">Returned if the user does not have the required Manager or HR roles.</response>
        [HttpGet]
        [Authorize(Roles = "Manager,HR")]
        public async Task<ActionResult<PagedResponse<LeaveRequestSummaryDto>>> GetAll([FromQuery] LeaveRequestPaginationParams pagination,
            [FromQuery] LeaveRequestSortParams sortParams,
            [FromQuery] LeaveRequestFilterParams filterParams)
        {
            var result = await _service.GetAllAsync(pagination, sortParams, filterParams);

            var response = new PagedResponse<LeaveRequestSummaryDto>() 
            {
                Items = result.Item2,
                TotalCount = result.Item1,
                PageSize = pagination.PageSize,
                Page = pagination.Page,
            };

            return Ok(response);
        }

        /// <summary>
        /// Approves a pending leave request.
        /// </summary>
        /// <param name="id">The unique identifier of the leave request.</param>
        /// <param name="dto">The approval request payload containing comments.</param>
        /// <returns>The updated leave request detail.</returns>
        /// <response code="200">Returned when the leave request has been successfully approved.</response>
        /// <response code="400">Returned if the request status transition is invalid.</response>
        /// <response code="401">Returned if the user is unauthenticated.</response>
        /// <response code="403">Returned if the user is not in the Manager role.</response>
        /// <response code="404">Returned if the leave request does not exist.</response>
        [HttpPut("{id}/approve")]
        [Authorize(Roles = "Manager")]
        public async Task<ActionResult<LeaveRequestDto>> Approve([FromRoute] Guid id, [FromBody] ApproveLeaveRequestDto dto) {

            int employeeId = GetCurrentUserId();
            var result = await _service.ApproveLeaveRequestAsync(id, employeeId, dto.Comments);

            if (result == null)
                return NotFound();

            return Ok(result);
        }

        /// <summary>
        /// Rejects a pending leave request.
        /// </summary>
        /// <param name="id">The unique identifier of the leave request.</param>
        /// <param name="dto">The rejection request payload containing comments.</param>
        /// <returns>The updated leave request detail.</returns>
        /// <response code="200">Returned when the leave request has been successfully rejected.</response>
        /// <response code="400">Returned if the request status transition is invalid.</response>
        /// <response code="401">Returned if the user is unauthenticated.</response>
        /// <response code="403">Returned if the user is not in the Manager role.</response>
        /// <response code="404">Returned if the leave request does not exist.</response>
        [HttpPut("{id}/reject")]
        [Authorize(Roles = "Manager")]
        public async Task<ActionResult<LeaveRequestDto>> Reject([FromRoute] Guid id, [FromBody] RejectLeaveRequestDto dto) {

            int employeeId = GetCurrentUserId();
            var result = await _service.RejectLeaveRequestAsync(id, employeeId, dto.Comments);

            if (result == null)
                return NotFound();

            return Ok(result);
        }

        /// <summary>
        /// Cancels a pending or approved leave request.
        /// </summary>
        /// <param name="id">The unique identifier of the leave request.</param>
        /// <param name="dto">The cancellation payload containing comments.</param>
        /// <returns>The updated leave request detail.</returns>
        /// <response code="200">Returned when the leave request has been successfully canceled.</response>
        /// <response code="400">Returned if the request status transition is invalid.</response>
        /// <response code="401">Returned if the user is unauthenticated.</response>
        /// <response code="404">Returned if the leave request does not exist.</response>
        [HttpPut("{id}/cancel")]
        public async Task<ActionResult<LeaveRequestDto>> Cancel([FromRoute] Guid id, [FromBody] CancelLeaveRequestDto dto)
        {

            int employeeId = GetCurrentUserId();
            var result = await _service.CancelLeaveRequestAsync(id, employeeId, dto.Comments);

            if (result == null)
                return NotFound();

            return Ok(result);
        }

        private int GetCurrentUserId() 
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
            {
                throw new UnauthorizedAccessException("User ID not found in token.");
            }
            return userId;
        }
    }
}
