using System;
using System.Collections.Generic;
using System.Text;

namespace LeaveManagement.Domain.Enums
{
    public class LeaveRequest
    {
        public int EmployeeId { get; set; }
        public int LeaveStatus { get; set; }
        public ICollection<Approvals> Approval { get; set; }
    }

    public class Approvals 
    {
        public int ApprovalId { get; set; }
        public int Approver { get; set; }
        public DateTime ProcessDateTime { get; set; }
        public string? Comments { get; set; }
    }
}
