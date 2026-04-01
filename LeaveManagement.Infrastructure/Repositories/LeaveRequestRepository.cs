using LeaveManagement.Application.Common;
using LeaveManagement.Application.Enums;
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
        
        public async Task<(int, IEnumerable<LeaveRequest>)> GetAllAsync(int pageSize, 
            int page, 
            LeaveRequestSortParams sortParams,
            LeaveRequestFilterParams filterParams)
        {
            var query = _context.LeaveRequests
                .Include(lr => lr.LeaveAudits)
                .AsQueryable();

            if (filterParams.Status.HasValue)
            {
                query = query.Where(lr => lr.LeaveStatus == filterParams.Status);
            }

            if (filterParams.Type.HasValue)
            {
                query = query.Where(lr => lr.LeaveType == filterParams.Type);
            }

            if (filterParams.EmployeeId.HasValue)
            {
                query = query.Where(lr => lr.EmployeeId == filterParams.EmployeeId);
            }

            if (filterParams.FromDate.HasValue)
            {
                query = query.Where(lr => lr.SubmittedDate >= filterParams.FromDate);
            }
            
            if(filterParams.ToDate.HasValue)
            {
                query = query.Where(lr => lr.SubmittedDate <= filterParams.ToDate);
            }

            query = sortParams.Field switch
            {
                SortByField.SubmittedDate => sortParams.Direction == SortDirection.Ascending
                    ? query.OrderBy(lr => lr.SubmittedDate)
                    : query.OrderByDescending(lr => lr.SubmittedDate),

                SortByField.NoOfDays => sortParams.Direction == SortDirection.Ascending
                    ? query.OrderBy(lr => (lr.EndDate - lr.StartDate))
                    : query.OrderByDescending(lr => (lr.EndDate - lr.StartDate)),
                _ => query.OrderBy(lr => lr.SubmittedDate)
            };

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (totalCount, items);
        }

        public async Task<(int, IEnumerable<LeaveRequest>)> GetByEmployeeIdAsync(int employeeId, 
            int pageSize, 
            int page, 
            LeaveRequestSortParams sortParams, 
            LeaveRequestFilterParams filterParams)
        {
            var query = _context.LeaveRequests
                .Include(lr => lr.LeaveAudits)
                .Where(lr => lr.EmployeeId == employeeId);

            if (filterParams.Status.HasValue)
            {
                query = query.Where(lr => lr.LeaveStatus == filterParams.Status);
            }

            if (filterParams.Type.HasValue)
            {
                query = query.Where(lr => lr.LeaveType == filterParams.Type);
            }

            if (filterParams.FromDate.HasValue)
            {
                query = query.Where(lr => lr.SubmittedDate > filterParams.FromDate);
            }
            
            if (filterParams.ToDate.HasValue)
            {
                query = query.Where(lr => lr.SubmittedDate < filterParams.ToDate);
            }

            query = sortParams.Field switch
            {
                SortByField.SubmittedDate => sortParams.Direction == SortDirection.Ascending
                    ? query.OrderBy(lr => lr.SubmittedDate)
                    : query.OrderByDescending(lr => lr.SubmittedDate),

                SortByField.NoOfDays => sortParams.Direction == SortDirection.Ascending
                    ? query.OrderBy(lr => (lr.EndDate - lr.StartDate))
                    : query.OrderByDescending(lr => (lr.EndDate - lr.StartDate)),
                _ => query.OrderBy(lr => lr.SubmittedDate)
            };

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (totalCount, items);
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
