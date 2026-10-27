using LeaveManagement.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace LeaveManagement.Infrastructure.Persistence
{
    public static class DataSeeder
    {
        public static async Task SeedAsync(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole<int>> roleManager)
        {
            // seed roles
            string[] roles = ["Employee", "Manager", "HR"];

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole<int>(role));
                }
            }

            var hr = await CreateUserIfNotExists(userManager, new ApplicationUser
            {
                UserName = "hr@test.com",
                Email = "hr@test.com",
                FirstName = "Bob",
                LastName = "HR"
            }, "Test@1234", "HR");

            var manager = await CreateUserIfNotExists(userManager, new ApplicationUser
            {
                UserName = "manager@test.com",
                Email = "manager@test.com",
                FirstName = "Jane",
                LastName = "Smith",
                ManagerId = hr?.Id,
            }, "Test@1234", "Manager");

            //seed test users
            await CreateUserIfNotExists(userManager, new ApplicationUser
            {
                UserName = "employee@test.com",
                Email = "employee@test.com",
                FirstName = "John",
                LastName = "Doe",
                ManagerId = manager?.Id,
            }, "Test@1234", "Employee");
        }

        private static async Task<ApplicationUser?> CreateUserIfNotExists(UserManager<ApplicationUser> userManager, ApplicationUser user, string password, string role)
        {
            var existing = await userManager.FindByEmailAsync(user.Email);
            if(existing != null) return existing;

            var result = await userManager.CreateAsync(user, password);

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, role);
                return user;
            }

            return null;
        }

    }
}
