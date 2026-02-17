using System;
using System.Collections.Generic;
using System.Text;

namespace LeaveManagement.Domain.Enums
{
    internal class LeaveStatus
    {
        public enum LeaveStatuses { 
            Pending,
            Approved,
            Rejected,
            Canceled
        }
    }
}
