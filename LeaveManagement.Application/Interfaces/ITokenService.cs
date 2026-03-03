using LeaveManagement.Domain.Entities;

namespace LeaveManagement.Application.Interfaces
{
    public interface ITokenService
    {
        string GenerateToken(ApplicationUser user, IEnumerable<string> roles);
    }
}
