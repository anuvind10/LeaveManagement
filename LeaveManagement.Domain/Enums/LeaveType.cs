using System;
using System.Collections.Generic;
using System.Text;

namespace LeaveManagement.Domain.Enums
{
    public class LeaveType
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
