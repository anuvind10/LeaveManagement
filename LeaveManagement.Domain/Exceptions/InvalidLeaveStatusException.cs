using LeaveManagement.Domain.Enums;

namespace LeaveManagement.Domain.Exceptions
{
    public class InvalidLeaveStatusException : DomainException
    {
        public InvalidLeaveStatusException(string action, LeaveStatus currentStatus) 
            : base($"Cannot {action} a leave request with status {currentStatus}.") {}
    }
}
