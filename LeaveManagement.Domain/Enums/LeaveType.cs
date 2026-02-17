using System;
using System.Collections.Generic;
using System.Text;

namespace LeaveManagement.Domain.Enums
{
    internal class LeaveType
    {
        public enum LeaveTypes { 
            Annual,
            Sick,
            Unpaid,
            Maternity,
            Paternity
        }
    }
}
