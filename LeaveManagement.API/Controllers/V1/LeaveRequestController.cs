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
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}[controller]")]
    [Authorize]
    public class LeaveRequestController : ControllerBase
    {
        private readonly ILeaveRequestService _service;

        public LeaveRequestController(ILeaveRequestService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<ActionResult<LeaveRequestDto>> SubmitLeaveRequest(CreateLeaveRequestDto dto) 
        {
            int employeeId = GetCurrentUserId();
            var result = await _service.SubmitLeaveRequestAsync(dto, employeeId);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<LeaveRequestDto>> GetById(Guid id) 
        {
            var result = await _service.GetByIdAsync(id);

            if (result == null)
                return NotFound();

            return Ok(result);
        }

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

        [HttpPut("{id}/approve")]
        [Authorize(Roles = "Manager")]
        public async Task<ActionResult<LeaveRequestDto>> Approve([FromRoute] Guid id, [FromBody] ApproveLeaveRequestDto dto) {

            int employeeId = GetCurrentUserId();
            var result = await _service.ApproveLeaveRequestAsync(id, employeeId, dto.Comments);

            if (result == null)
                return NotFound();

            return Ok(result);
        }

        [HttpPut("{id}/reject")]
        [Authorize(Roles = "Manager")]
        public async Task<ActionResult<LeaveRequestDto>> Reject([FromRoute] Guid id, [FromBody] RejectLeaveRequestDto dto) {

            int employeeId = GetCurrentUserId();
            var result = await _service.RejectLeaveRequestAsync(id, employeeId, dto.Comments);

            if (result == null)
                return NotFound();

            return Ok(result);
        }

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
