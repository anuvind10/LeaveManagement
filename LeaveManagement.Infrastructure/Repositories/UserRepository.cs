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
        public async Task<bool> IsManagerAsync(int employeeId, int managerId)
        {
            return await _context.Users.AnyAsync(u => u.Id == employeeId && u.ManagerId == managerId);
        }

        public async Task<IEnumerable<int>> GetReporteesAsync(int managerId)
        {
            return await _context.Users
                .Where(u => u.ManagerId == managerId)
                .Select(u => u.Id)
                .ToListAsync();
        }
    }
}
