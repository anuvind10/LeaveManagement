using System;
using System.Collections.Generic;
using System.Text;

namespace LeaveManagement.Domain.Enums
{
    internal class LeaveRequest
    {
        public int EmployeeId { get; set; }
        public int LeaveStatus { get; set; }
        public Approvals Approval { get; set; }
    }

    internal class Approvals 
    {
        public int ApprovalId { get; set; }
        public int Approver { get; set; }
        public DateTime ProcessDateTime { get; set; }
        public string? Comments { get; set; }
    }
}
