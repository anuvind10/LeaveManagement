using LeaveManagement.Domain.Enums;

namespace LeaveManagement.Application.Common
{
    public class LeaveRequestFilterParams
    {
        public LeaveStatus? Status { get; set; }
        public LeaveType? Type { get; set; }
        public int? EmployeeId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}
