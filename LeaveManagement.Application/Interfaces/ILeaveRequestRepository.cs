using LeaveManagement.Domain.Entities;
using LeaveManagement.Domain.Enums;

namespace LeaveManagement.Application.Interfaces
{
    public interface ILeaveRequestRepository
    {
        Task<LeaveRequest?> GetByIdAsync(Guid id);
        Task<IEnumerable<LeaveRequest>> GetAllAsync();
        Task<IEnumerable<LeaveRequest>> GetByEmployeeIdAsync(int employeeId);
        Task<IEnumerable<LeaveRequest>> GetByStatusAsync(LeaveStatus status);
        Task CreateAsync(LeaveRequest leaveRequest);
        Task UpdateAsync(LeaveRequest leaveRequest);
        Task DeleteAsync(Guid id);
    }
}
