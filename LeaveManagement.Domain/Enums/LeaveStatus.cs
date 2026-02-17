using System;
using System.Collections.Generic;
using System.Text;

namespace LeaveManagement.Domain.Enums
{
    public class LeaveStatus
    {
        public enum LeaveStatuses { 
            Pending,
            Approved,
            Rejected,
            Canceled
        }
    }
}
