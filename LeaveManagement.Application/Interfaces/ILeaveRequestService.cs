using LeaveManagement.Application.Common;
using LeaveManagement.Application.DTOs;
using LeaveManagement.Domain.Enums;

namespace LeaveManagement.Application.Interfaces
{
    public interface ILeaveRequestService
    {
        Task<LeaveRequestDto> SubmitLeaveRequestAsync(CreateLeaveRequestDto dto, int employeeId);
        Task<LeaveRequestDto?> GetByIdAsync(Guid id);
        Task<LeaveRequestDto?> ApproveLeaveRequestAsync(Guid id, int auditorId, string? comments);
        Task<LeaveRequestDto?> RejectLeaveRequestAsync(Guid id, int auditorId, string comments);
        Task<LeaveRequestDto?> CancelLeaveRequestAsync(Guid id, int auditorId, string? comments);
        Task<(int, IEnumerable<LeaveRequestSummaryDto>)> GetAllAsync(LeaveStatus? status, PaginationParams pagination);
        Task<(int, IEnumerable<LeaveRequestSummaryDto>)> GetByEmployeeIdAsync(int employeeId, PaginationParams pagination);
    }
}
