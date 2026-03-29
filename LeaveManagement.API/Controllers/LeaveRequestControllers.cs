using LeaveManagement.API.Models;
using LeaveManagement.Application.Common;
using LeaveManagement.Application.DTOs;
using LeaveManagement.Application.Interfaces;
using LeaveManagement.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.ResponseCaching;
using System.Security.Claims;

namespace LeaveManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class LeaveRequestControllers : ControllerBase
    {
        private readonly ILeaveRequestService _service;

        public LeaveRequestControllers(ILeaveRequestService service)
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
        public async Task<ActionResult<(int, IEnumerable<LeaveRequestSummaryDto>)>> GetByEmployeeId(int employeeId, [FromQuery] PaginationParams paginationParams)
        {
            var result = await _service.GetByEmployeeIdAsync(employeeId, paginationParams.PageSize, paginationParams.Page);

            var response = new PagedResponse()
            {
                dtos = result.Item2,
                TotalCount = result.Item1,
                PageSize = paginationParams.PageSize,
                Page = paginationParams.Page,
            };

            return Ok(response);
        }

        [HttpGet]
        [Authorize(Roles = "Manager,HR")]
        public async Task<ActionResult<PaginationParams>> GetAll(LeaveStatus? status, [FromQuery] PaginationParams paginationParams)
        {
            var result = await _service.GetAllAsync(status, paginationParams.PageSize, paginationParams.Page);

            var response = new PagedResponse() 
            {
                dtos = result.Item2,
                TotalCount = result.Item1,
                PageSize = paginationParams.PageSize,
                Page = paginationParams.Page,
            };

            return Ok(response);    ;
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
