using LeaveManagement.Domain.Entities;
using LeaveManagement.Domain.Enums;
using LeaveManagement.Domain.Exceptions;

namespace LeaveManagement.Tests.Domain
{
    public class LeaveRequestTests
    {
        [Fact]
        public void Approve_WhenStatusIsPending_ShouldTransitionToApproved()
        {
            var leaveRequest = new LeaveRequest
            {
                Id = Guid.NewGuid(),
                SubmittedDate = DateTime.UtcNow,
                EmployeeId = 1,
                LeaveType = LeaveType.Sick,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(2),
                LeaveStatus = LeaveStatus.Pending,
            };

            leaveRequest.Approve(auditorId: 2, comments: "Looks good");

            Assert.Equal(LeaveStatus.Approved, leaveRequest.LeaveStatus);
        }

        [Fact]
        public void Approve_WhenStatusIsPending_ShouldCreateAuditRecord()
        {
            var leaveRequest = new LeaveRequest
            {
                Id = Guid.NewGuid(),
                SubmittedDate = DateTime.UtcNow,
                EmployeeId = 2,
                LeaveType = LeaveType.Annual,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(5),
                LeaveStatus = LeaveStatus.Pending,
            };

            leaveRequest.Approve(auditorId: 3, comments: "Looks good");

            Assert.Single(leaveRequest.LeaveAudits);

            var audit = leaveRequest.LeaveAudits.First();

            Assert.Equal(3, audit.AuditorId);
            Assert.Equal(LeaveAction.Approved, audit.Action);
            Assert.Equal("Looks good", audit.Comments);
        }

        [Fact]
        public void Approve_WhenStatusIsNotPending_ShouldThrowInvalidLeaveStatusException()
        {
            var leaveRequest = new LeaveRequest
            {
                Id = Guid.NewGuid(),
                SubmittedDate = DateTime.UtcNow,
                EmployeeId = 2,
                LeaveType = LeaveType.Annual,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(5),
                LeaveStatus = LeaveStatus.Canceled,
            };

            Assert.Throws<InvalidLeaveStatusException>(() => leaveRequest.Approve(2, ""));
        }
    }
}
