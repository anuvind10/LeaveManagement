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
        public ICollection<LeaveAudits> LeaveAudits { get; set; } = new List<LeaveAudits>();
        public void Approve(int auditorId, string? comments) {
            if (LeaveStatus != LeaveStatus.Pending)
            {
                throw new InvalidLeaveStatusException(nameof(Approve), LeaveStatus);
            }

            CreateApproval(auditorId, comments, LeaveAction.Approved);
            LeaveStatus = LeaveStatus.Approved;
        }

        public void Reject(int auditorId, string comments) 
        {
            if (LeaveStatus != LeaveStatus.Pending)
            {
                throw new InvalidLeaveStatusException(nameof(Reject), LeaveStatus);
            }

            CreateApproval(auditorId, comments, LeaveAction.Rejected);
            LeaveStatus = LeaveStatus.Rejected;
        }

        public void Cancel(int auditorId, string? comments) {
            if (LeaveStatus != LeaveStatus.Pending &&
                LeaveStatus != LeaveStatus.Approved) {
                throw new InvalidLeaveStatusException(nameof(Cancel), LeaveStatus);
            }

            CreateApproval(auditorId, comments, LeaveAction.Canceled);
            LeaveStatus = LeaveStatus.Canceled;
        }

        private void CreateApproval(int auditorId, string? comments, LeaveAction action) {
            var approval = new LeaveAudits()
            {
                LeaveRequestId = Id,
                AuditorId = auditorId,
                ProcessDateTime = DateTime.UtcNow,
                Comments = comments,
                Action = action
            };

            LeaveAudits.Add(approval);
        }
    }
}
