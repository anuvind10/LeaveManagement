using LeaveManagement.Domain.Enums;

namespace LeaveManagement.Application.DTOs
{
    public class LeaveRequestSummaryDto
    {
        public Guid Id { get; set; }
        public LeaveType LeaveType { get; set; }
        public decimal NoOfDays { get; set; }
        public string? Reason { get; set; }
        public LeaveStatus LeaveStatus { get; set; }
    }
}

