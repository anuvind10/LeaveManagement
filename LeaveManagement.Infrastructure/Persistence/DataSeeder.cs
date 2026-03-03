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

            //seed test users
            await CreateUserIfNotExists(userManager, new ApplicationUser
            {
                UserName = "employee@test.com",
                Email = "employee@test.com",
                FirstName = "John",
                LastName = "Doe"
            }, "Test@1234", "Employee");

            await CreateUserIfNotExists(userManager, new ApplicationUser
            {
                UserName = "manager@test.com",
                Email = "manager@test.com",
                FirstName = "Jane",
                LastName = "Smith"
            }, "Test@1234", "Manager");

            await CreateUserIfNotExists(userManager, new ApplicationUser
            {
                UserName = "hr@test.com",
                Email = "hr@test.com",
                FirstName = "Bob",
                LastName = "HR"
            }, "Test@1234", "HR");
        }

        private static async Task CreateUserIfNotExists(UserManager<ApplicationUser> userManager, ApplicationUser user, string password, string role)
        {
            if (await userManager.FindByEmailAsync(user.Email!) == null) 
            {
                var result = await userManager.CreateAsync(user, password);
                if (result.Succeeded)
                { 
                    await userManager.AddToRoleAsync(user, role);
                }
            }
        }

    }
}
