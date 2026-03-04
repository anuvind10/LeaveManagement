using LeaveManagement.Application.DTOs;
using LeaveManagement.Application.Services;
using LeaveManagement.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LeaveManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class LeaveRequestControllers : ControllerBase
    {
        private readonly LeaveRequestService _service;

        public LeaveRequestControllers(LeaveRequestService service)
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
        public async Task<ActionResult<IEnumerable<LeaveRequestSummaryDto>>> GetByEmployeeId(int employeeId)
        {
            var result = await _service.GetByEmployeeIdAsync(employeeId);

            return Ok(result);
        }

        [HttpGet]
        [Authorize(Roles = "Manager,HR")]
        public async Task<ActionResult<IEnumerable<LeaveRequestSummaryDto>>> GetAll(LeaveStatus? status)
        {
            IEnumerable<LeaveRequestSummaryDto> result;
            if (status.HasValue)
            {
                result = await _service.GetByStatusAsync(status.Value);
            }
            else
            {
                result = await _service.GetAllAsync();
            }

            return Ok(result);
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
