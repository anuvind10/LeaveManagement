using LeaveManagement.Domain.Entities;

namespace LeaveManagement.Application.Interfaces
{
    public interface IUserRepository
    {
        Task<bool> IsManagerAsync(int employeeId, int managerId);
        Task<IEnumerable<int>> GetReporteesAsync (int managerId);
    }
}
