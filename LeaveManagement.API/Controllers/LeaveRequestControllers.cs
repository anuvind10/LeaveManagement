using LeaveManagement.Application.DTOs;
using LeaveManagement.Application.Services;
using LeaveManagement.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace LeaveManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
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
            try
            {
                int employeeId = 1;
                var result = await _service.SubmitLeaveRequestAsync(dto, employeeId);
                return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
            }
            catch(ArgumentException ex) {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<LeaveRequestDto>> GetById(Guid id) 
        {
            var result = await _service.GetByIdAsync(id);

            if (result == null)
                return NotFound();

            return Ok(result);
        }

        [HttpPut("{id}/approve")]
        public async Task<ActionResult<LeaveRequestDto>> Approve([FromRoute] Guid id, [FromBody] ApproveLeaveRequestDto dto) {
            try
            {
                int approverId = 2;
                var result = await _service.ApproveLeaveRequestAsync(id, approverId, dto.Comments);

                if (result == null)
                    return NotFound();

                return Ok(result);
            }
            catch (DomainException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
