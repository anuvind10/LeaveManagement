using LeaveManagement.Domain.Entities;
using LeaveManagement.Domain.Enums;
using LeaveManagement.Domain.Exceptions;

namespace LeaveManagement.Tests.Domain
{
    public class LeaveRequestTests
    {
        private readonly LeaveRequest _pendingLeaveRequest;
        private readonly LeaveRequest _approvedLeaveRequest;

        public LeaveRequestTests()
        {
            _pendingLeaveRequest = new LeaveRequest
            {
                Id = Guid.NewGuid(),
                SubmittedDate = DateTime.UtcNow,
                EmployeeId = 1,
                LeaveType = LeaveType.Sick,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(2),
                LeaveStatus = LeaveStatus.Pending,
            };

            _approvedLeaveRequest = new LeaveRequest
            {
                Id = Guid.NewGuid(),
                SubmittedDate = DateTime.UtcNow,
                EmployeeId = 2,
                LeaveType = LeaveType.Annual,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(5),
                LeaveStatus = LeaveStatus.Canceled,
            };
        }

        [Fact]
        public void Approve_WhenStatusIsPending_ShouldTransitionToApproved()
        {
            var leaveRequest = _pendingLeaveRequest;

            leaveRequest.Approve(auditorId: 2, comments: "Looks good");

            Assert.Equal(LeaveStatus.Approved, leaveRequest.LeaveStatus);
        }

        [Fact]
        public void Approve_WhenStatusIsPending_ShouldCreateAuditRecord()
        {
            var leaveRequest = _pendingLeaveRequest;

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
            var leaveRequest = _approvedLeaveRequest;

            Assert.Throws<InvalidLeaveStatusException>(() => leaveRequest.Approve(2, ""));
        }
    }
}
