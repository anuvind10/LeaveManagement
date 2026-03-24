using LeaveManagement.Application.DTOs;
using LeaveManagement.Application.Interfaces;
using LeaveManagement.Domain.Entities;
using Riok.Mapperly.Abstractions;

namespace LeaveManagement.Application.Mappings
{
    [Mapper]
    public partial class LeaveRequestMapper : ILeaveRequestMapper
    {
        [MapperIgnoreSource(nameof(LeaveRequest.EmployeeId))]
        [MapperIgnoreSource(nameof(LeaveRequest.RowVersion))]
        public partial LeaveRequestDto ToDto(LeaveRequest source);

        [MapperIgnoreSource(nameof(LeaveRequest.RowVersion))]
        public partial IEnumerable<LeaveRequestSummaryDto> ToSummaryDtoList(IEnumerable<LeaveRequest> source);

        [MapperIgnoreSource(nameof(LeaveRequest.SubmittedDate))]
        [MapperIgnoreSource(nameof(LeaveRequest.EmployeeId))]
        [MapperIgnoreSource(nameof(LeaveRequest.StartDate))]
        [MapperIgnoreSource(nameof(LeaveRequest.EndDate))]
        [MapperIgnoreSource(nameof(LeaveRequest.LeaveAudits))]
        [MapperIgnoreSource(nameof(LeaveRequest.RowVersion))]
        private partial LeaveRequestSummaryDto ToSummaryDto(LeaveRequest source);

        [MapperIgnoreSource(nameof(LeaveAudit.LeaveRequestId))]
        private partial LeaveAuditDto ToAuditDto(LeaveAudit source);
    }
}
