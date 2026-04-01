using LeaveManagement.Application.Common;
using LeaveManagement.Domain.Entities;
using LeaveManagement.Domain.Enums;

namespace LeaveManagement.Application.Interfaces
{
    public interface ILeaveRequestRepository
    {
        Task<LeaveRequest?> GetByIdAsync(Guid id);
        Task<(int, IEnumerable<LeaveRequest>)> GetAllAsync(int pageSize, int page, LeaveRequestSortParams sortParams, LeaveRequestFilterParams filterParams);
        Task<(int, IEnumerable<LeaveRequest>)> GetByEmployeeIdAsync(int employeeId, int pageSize, int page, LeaveRequestSortParams sortParams, LeaveRequestFilterParams filterParams);
        Task CreateAsync(LeaveRequest leaveRequest);
        Task UpdateAsync(LeaveRequest leaveRequest);
        Task DeleteAsync(Guid id);
    }
}
