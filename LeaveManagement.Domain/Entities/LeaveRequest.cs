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

            CreateApproval(approverId, comments, ApprovalAction.Rejected);
            LeaveStatus = LeaveStatus.Approved;
        }

        public void Reject(int rejectorId, string comments) 
        {
            if (LeaveStatus != LeaveStatus.Pending)
            {
                throw new InvalidLeaveStatusException(nameof(Reject), LeaveStatus);
            }

            CreateApproval(rejectorId, comments, ApprovalAction.Rejected);
            LeaveStatus = LeaveStatus.Rejected;
        }

        private void CreateApproval(int approverId, string? comments, ApprovalAction action) {
            var approval = new Approval()
            {
                LeaveRequestId = Id,
                ApproverId = approverId,
                ProcessDateTime = DateTime.UtcNow,
                Comments = comments,
                Action = action
            };

            Approvals.Add(approval);
        }
    }
}
