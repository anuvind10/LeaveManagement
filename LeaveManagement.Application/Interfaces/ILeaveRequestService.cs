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
        Task<(int, IEnumerable<LeaveRequestSummaryDto>)> GetAllAsync(LeaveStatus? status, int pageSize, int page);
        Task<IEnumerable<LeaveRequestSummaryDto>?> GetByEmployeeIdAsync(int employeeId);
    }
}
