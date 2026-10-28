using LeaveManagement.Application.Interfaces;
using LeaveManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LeaveManagement.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;
        public UserRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task<bool> IsManager(int employeeId, int managerId)
        {
            return await _context.Users.AnyAsync(u => u.Id == employeeId && u.ManagerId == managerId);
        }
    }
}
