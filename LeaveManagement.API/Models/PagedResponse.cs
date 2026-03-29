using LeaveManagement.Application.DTOs;

namespace LeaveManagement.API.Models
{
    public class PagedResponse
    {
        public IEnumerable<LeaveRequestSummaryDto> dtos { get; set; }
        public int TotalCount { get; set; }
        public int PageSize { get; set; }
        public int Page { get; set; }
    }
}
