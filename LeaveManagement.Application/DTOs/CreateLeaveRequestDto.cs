using LeaveManagement.Domain.Enums;

namespace LeaveManagement.Application.DTOs
{
    public class CreateLeaveRequestDto
    {
        public LeaveType LeaveType { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? Reason { get; set; }
    }
}
