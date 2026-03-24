using LeaveManagement.Application.DTOs;
using LeaveManagement.Domain.Entities;

namespace LeaveManagement.Application.Interfaces
{
    public interface ILeaveRequestMapper
    {
        LeaveRequestDto ToDto(LeaveRequest source);
        IEnumerable<LeaveRequestSummaryDto> ToSummaryDtoList(IEnumerable<LeaveRequest> source);
    }
}
