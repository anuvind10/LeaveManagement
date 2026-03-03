using LeaveManagement.Application.DTOs;
using LeaveManagement.Application.Interfaces;
using LeaveManagement.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace LeaveManagement.Application.Services
{
    public class AuthService
    {
        private readonly ITokenService _tokenService;
        private readonly UserManager<ApplicationUser> _userManager;
        
        public AuthService(ITokenService tokenService, UserManager<ApplicationUser> usermanager)
        {
            _tokenService = tokenService;
            _userManager = usermanager;
        }
        public async Task<string?> LoginAsync(LoginDto dto)
        { 
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null) { 
                return null;
            }

            if (await _userManager.CheckPasswordAsync(user, dto.Password))
            {
                var roles = await _userManager.GetRolesAsync(user);
                var token = _tokenService.GenerateToken(user, roles);
                return token;
            }

            return null;
        }
    }
}
