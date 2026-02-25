using LeaveManagement.Domain.Enums;

namespace LeaveManagement.Application.DTOs
{
    public class LeaveRequestDto
    {
        public Guid Id { get; set; }
        public DateTime SubmittedDate { get; set; }
        public LeaveType LeaveType { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal NoOfDays { get; set; }
        public string? Reason { get; set; }
        public LeaveStatus LeaveStatus { get; set; }
        public List<ApprovalDto>? Approvals { get; set; }
    }
    public class ApprovalDto
    {
        public Guid ApprovalId { get; set; }
        public int ApproverId { get; set; }
        public DateTime ProcessDateTime { get; set; }
        public string? Comments { get; set; }
    }
}
