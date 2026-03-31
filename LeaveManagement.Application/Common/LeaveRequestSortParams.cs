using LeaveManagement.Application.Enums;

namespace LeaveManagement.Application.Common
{
    public class LeaveRequestSortParams
    {
        public SortByField Field { get; set; } = SortByField.SubmittedDate;
        public SortDirection Direction { get; set; } = SortDirection.Ascending;
    }
}
