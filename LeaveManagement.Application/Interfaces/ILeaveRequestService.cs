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
        Task<IEnumerable<LeaveRequestSummaryDto>> GetAllAsync();
        Task<IEnumerable<LeaveRequestSummaryDto>> GetByStatusAsync(LeaveStatus status);
        Task<IEnumerable<LeaveRequestSummaryDto>> GetByEmployeeIdAsync(int employeeId);
    }
}
