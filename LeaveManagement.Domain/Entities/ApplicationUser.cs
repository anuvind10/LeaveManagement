using Microsoft.AspNetCore.Identity;

namespace LeaveManagement.Domain.Entities
{
    public class ApplicationUser:IdentityUser<int>
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
    }
}
