namespace LeaveManagement.Application.Interfaces
{
    public interface ICurrentUserService
    {
        public int UserId { get; }
        bool IsInRole(string role);
    }
}
