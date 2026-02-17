using LeaveManagement.Domain.Enums;

namespace LeaveManagement.Domain.Entities
{
    public class LeaveRequest
    {
        public Guid Id { get; set; }
        public DateTime SubmittedDate { get; set; }
        public int EmployeeId { get; set; }
        public LeaveType LeaveType { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal NoOfDays { get; set; }
        public string? Reason { get; set; }
        public LeaveStatus LeaveStatus { get; set; }
        public ICollection<Approval> Approvals { get; set; } = new List<Approval>();
    }
}
