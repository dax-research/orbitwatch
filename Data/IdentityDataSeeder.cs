using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using OrbitWatch.Models;

namespace OrbitWatch.Data
{
    public static class IdentityDataSeeder
    {
        public static async Task SeedRolesAndUsersAsync(
            RoleManager<IdentityRole> roleManager, 
            UserManager<ApplicationUser> userManager, 
            ILogger logger)
        {
            var roles = new[] { "Admin", "Manager", "Analyst" };
            
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // Bootstrap Admin
            await AssignRoleToUser(userManager, logger, "admin@test.com", "Admin", warnIfMissing: true);

            // Bootstrap Manager
            await AssignRoleToUser(userManager, logger, "manager@test.com", "Manager", warnIfMissing: true);

            // Bootstrap Analyst
            await AssignRoleToUser(userManager, logger, "analyst@test.com", "Analyst", warnIfMissing: false);
        }

        private static async Task AssignRoleToUser(
            UserManager<ApplicationUser> userManager, 
            ILogger logger, 
            string email, 
            string role, 
            bool warnIfMissing)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                if (warnIfMissing)
                {
                    logger.LogWarning("Bootstrap user {Email} was not found. Expected to assign {Role} role.", email, role);
                }
                else
                {
                    logger.LogInformation("Bootstrap user {Email} not found. Skipping {Role} assignment.", email, role);
                }
                return;
            }

            if (!await userManager.IsInRoleAsync(user, role))
            {
                var result = await userManager.AddToRoleAsync(user, role);
                if (result.Succeeded)
                {
                    logger.LogInformation("Assigned {Role} role to bootstrap user {Email}.", role, email);
                }
                else
                {
                    logger.LogError("Failed to assign {Role} to {Email}: {Errors}", role, email, string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
            else
            {
                logger.LogInformation("User {Email} already has {Role} role.", email, role);
            }
        }
    }
}
