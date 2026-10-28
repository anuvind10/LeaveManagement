namespace LeaveManagement.Application.Interfaces
{
    public interface IUserRepository
    {
        Task<bool> IsManager (int employeeId, int managerId);
    }
}
