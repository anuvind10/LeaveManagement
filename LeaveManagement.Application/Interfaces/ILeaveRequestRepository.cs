using LeaveManagement.Domain.Entities;
using LeaveManagement.Domain.Enums;

namespace LeaveManagement.Application.Interfaces
{
    public interface ILeaveRequestRepository
    {
        Task<LeaveRequest?> GetByIdAsync(Guid id);
        Task<(int, IEnumerable<LeaveRequest>)> GetAllAsync(LeaveStatus? status, int pageSize, int page);
        Task<(int, IEnumerable<LeaveRequest>)> GetByEmployeeIdAsync(int employeeId, int pageSize, int page);
        Task CreateAsync(LeaveRequest leaveRequest);
        Task UpdateAsync(LeaveRequest leaveRequest);
        Task DeleteAsync(Guid id);
    }
}
