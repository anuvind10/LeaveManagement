using LeaveManagement.Domain.Entities;
using LeaveManagement.Domain.Enums;
using LeaveManagement.Domain.Exceptions;

namespace LeaveManagement.Tests.Domain
{
    public class LeaveRequestTests
    {
        private readonly LeaveRequest _pendingLeaveRequest;
        private readonly LeaveRequest _approvedLeaveRequest;
        private readonly LeaveRequest _rejectedLeaveRequest;

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
                LeaveStatus = LeaveStatus.Approved,
            };

            _rejectedLeaveRequest = new LeaveRequest
            {
                Id = Guid.NewGuid(),
                SubmittedDate = DateTime.UtcNow,
                EmployeeId = 2,
                LeaveType = LeaveType.Annual,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(5),
                LeaveStatus = LeaveStatus.Rejected,
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

        [Fact]
        public void Reject_WhenStatusIsPending_ShouldTransitionToRejected()
        {
            var leaveRequest = _pendingLeaveRequest;

            leaveRequest.Reject(auditorId: 3, comments: "Cannot approve");

            Assert.Equal(LeaveStatus.Rejected, leaveRequest.LeaveStatus);
        }

        [Fact]
        public void Reject_WhenStatusIsNotPending_ShouldThrowInvalidLeaveStatusException()
        {
            var leaveRequest = _approvedLeaveRequest;

            Assert.Throws<InvalidLeaveStatusException>(() => leaveRequest.Reject(auditorId: 3, comments: "Sorry, rejecting"));
        }

        [Fact]
        public void Cancel_WhenStatusIsPending_ShouldTransitionToCanceled()
        {
            var leaveRequest = _pendingLeaveRequest;

            leaveRequest.Cancel(auditorId: 1, comments: "wrong leave entry");

            Assert.Equal(LeaveStatus.Canceled, leaveRequest.LeaveStatus);
        }

        [Fact]
        public void Cancel_WhenStatusIsNotPending_ShouldTransitionToCanceled()
        {
            var leaveRequest = _approvedLeaveRequest;

            leaveRequest.Cancel(auditorId: 1, comments: "Plans changed");

            Assert.Single(leaveRequest.LeaveAudits);

            var audit = leaveRequest.LeaveAudits.First();

            Assert.Equal(LeaveStatus.Canceled, leaveRequest.LeaveStatus);
            Assert.Equal("Plans changed", audit.Comments);
        }

        [Fact]
        public void Cancel_WhenStatusIsRejected_ShouldThrowInvalidLeaveStatusException()
        {
            var leaveRequest = _rejectedLeaveRequest;

            Assert.Throws<InvalidLeaveStatusException>(() => leaveRequest.Cancel(auditorId: 1, ""));
        }
    }
}
