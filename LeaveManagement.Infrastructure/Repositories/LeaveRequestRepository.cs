using LeaveManagement.Application.Interfaces;
using LeaveManagement.Domain.Entities;
using LeaveManagement.Domain.Enums;
using LeaveManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LeaveManagement.Infrastructure.Repositories
{
    public class LeaveRequestRepository : ILeaveRequestRepository
    {
        private readonly AppDbContext _context;

        public LeaveRequestRepository(AppDbContext context)
        {
            _context = context;
        }
        
        public async Task<(int, IEnumerable<LeaveRequest>)> GetAllAsync(LeaveStatus? status, int pageSize, int page)
        {
            var query = _context.LeaveRequests
                .Include(lr => lr.LeaveAudits)
                .OrderBy(lr => lr.Id)
                .AsQueryable();

            if (status.HasValue)
            {
                query = query.Where(lr => lr.LeaveStatus == status);
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new (totalCount, items);
        }

        public async Task<IEnumerable<LeaveRequest>> GetByEmployeeIdAsync(int employeeId)
        {
            return await _context.LeaveRequests
                .Include(lr => lr.LeaveAudits)
                .Where(lr => lr.EmployeeId == employeeId)
                .ToListAsync();
        }

        public async Task<LeaveRequest?> GetByIdAsync(Guid id)
        {
            return await _context.LeaveRequests
                .Include(lr => lr.LeaveAudits)
                .FirstOrDefaultAsync(lr => lr.Id == id);
        }
        public async Task CreateAsync(LeaveRequest leaveRequest)
        {
            await _context.LeaveRequests.AddAsync(leaveRequest);
            await _context.SaveChangesAsync();
        }
        public async Task UpdateAsync(LeaveRequest leaveRequest)
        {
            await _context.SaveChangesAsync();
        }
        public async Task DeleteAsync(Guid id)
        {
            var leaveRequest = await _context.LeaveRequests.FindAsync(id);
            if (leaveRequest != null) {
                _context.LeaveRequests.Remove(leaveRequest);
                await _context.SaveChangesAsync();
            }
        }
    }
}
