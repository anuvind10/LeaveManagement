using LeaveManagement.Domain.Enums;
using LeaveManagement.Domain.Exceptions;

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
        public void Approve(int approverId, string? comments) {
            if (LeaveStatus != LeaveStatus.Pending)
            {
                throw new InvalidLeaveStatusException(nameof(Approve), LeaveStatus);
            }

            // Create new approval and add it to the Approvals list
            var approval = new Approval()
            {
                ApprovalId = Guid.NewGuid(),
                ApproverId = approverId,
                ProcessDateTime = DateTime.UtcNow,
                Comments = comments
            };

            Approvals.Add(approval);

            // Approve the request
            LeaveStatus = LeaveStatus.Approved;
        }
    }
}
