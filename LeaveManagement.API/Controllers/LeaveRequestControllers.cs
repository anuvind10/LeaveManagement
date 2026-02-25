using LeaveManagement.Application.DTOs;
using LeaveManagement.Application.Services;
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
    }
}
