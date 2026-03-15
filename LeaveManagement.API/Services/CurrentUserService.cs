using LeaveManagement.Application.Interfaces;
using System.Security.Claims;

namespace LeaveManagement.API.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _contextAccessor;
        public CurrentUserService(IHttpContextAccessor contextAccessor)
        {
            _contextAccessor = contextAccessor;
        }

        public int UserId => GetUserId();
        public bool IsInRole(string role) => _contextAccessor.HttpContext?.User?.IsInRole(role) ?? false;

        private int GetUserId()
        {
            var userIdString = _contextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
            {
                throw new InvalidOperationException("User ID claim is missing or invalid.");
            }
            return userId;
        }
    }
}
