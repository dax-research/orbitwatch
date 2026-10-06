using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using OrbitWatch.Data;
using OrbitWatch.Models;
using Xunit;

namespace OrbitWatch.Tests
{
    public class IdentityDataSeederTests
    {
        private Mock<UserManager<ApplicationUser>> MockUserManager()
        {
            var store = new Mock<IUserStore<ApplicationUser>>();
            return new Mock<UserManager<ApplicationUser>>(store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
        }

        private Mock<RoleManager<IdentityRole>> MockRoleManager()
        {
            var store = new Mock<IRoleStore<IdentityRole>>();
            return new Mock<RoleManager<IdentityRole>>(store.Object, null!, null!, null!, null!);
        }

        private Mock<ILogger> MockLogger()
        {
            return new Mock<ILogger>();
        }

        [Fact]
        public async Task SeedRolesAndUsersAsync_CreatesRolesIfMissing()
        {
            var roleManager = MockRoleManager();
            var userManager = MockUserManager();
            var logger = MockLogger();

            roleManager.Setup(x => x.RoleExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
            roleManager.Setup(x => x.CreateAsync(It.IsAny<IdentityRole>())).ReturnsAsync(IdentityResult.Success);

            await IdentityDataSeeder.SeedRolesAndUsersAsync(roleManager.Object, userManager.Object, logger.Object);

            roleManager.Verify(x => x.CreateAsync(It.Is<IdentityRole>(r => r.Name == "Admin")), Times.Once);
            roleManager.Verify(x => x.CreateAsync(It.Is<IdentityRole>(r => r.Name == "Manager")), Times.Once);
            roleManager.Verify(x => x.CreateAsync(It.Is<IdentityRole>(r => r.Name == "Analyst")), Times.Once);
        }

        [Fact]
        public async Task SeedRolesAndUsersAsync_DoesNotCreateRolesIfTheyExist()
        {
            var roleManager = MockRoleManager();
            var userManager = MockUserManager();
            var logger = MockLogger();

            roleManager.Setup(x => x.RoleExistsAsync(It.IsAny<string>())).ReturnsAsync(true);

            await IdentityDataSeeder.SeedRolesAndUsersAsync(roleManager.Object, userManager.Object, logger.Object);

            roleManager.Verify(x => x.CreateAsync(It.IsAny<IdentityRole>()), Times.Never);
        }

        [Fact]
        public async Task SeedRolesAndUsersAsync_AssignsRoleWhenMissing()
        {
            var roleManager = MockRoleManager();
            var userManager = MockUserManager();
            var logger = MockLogger();

            roleManager.Setup(x => x.RoleExistsAsync(It.IsAny<string>())).ReturnsAsync(true);

            var adminUser = new ApplicationUser { Email = "admin@test.com" };
            userManager.Setup(x => x.FindByEmailAsync("admin@test.com")).ReturnsAsync(adminUser);
            userManager.Setup(x => x.IsInRoleAsync(adminUser, "Admin")).ReturnsAsync(false);
            userManager.Setup(x => x.AddToRoleAsync(adminUser, "Admin")).ReturnsAsync(IdentityResult.Success);

            await IdentityDataSeeder.SeedRolesAndUsersAsync(roleManager.Object, userManager.Object, logger.Object);

            userManager.Verify(x => x.AddToRoleAsync(adminUser, "Admin"), Times.Once);
        }

        [Fact]
        public async Task SeedRolesAndUsersAsync_DoesNotAssignRoleIfAlreadyPresent()
        {
            var roleManager = MockRoleManager();
            var userManager = MockUserManager();
            var logger = MockLogger();

            roleManager.Setup(x => x.RoleExistsAsync(It.IsAny<string>())).ReturnsAsync(true);

            var managerUser = new ApplicationUser { Email = "manager@test.com" };
            userManager.Setup(x => x.FindByEmailAsync("manager@test.com")).ReturnsAsync(managerUser);
            userManager.Setup(x => x.IsInRoleAsync(managerUser, "Manager")).ReturnsAsync(true);

            await IdentityDataSeeder.SeedRolesAndUsersAsync(roleManager.Object, userManager.Object, logger.Object);

            userManager.Verify(x => x.AddToRoleAsync(managerUser, "Manager"), Times.Never);
        }

        [Fact]
        public async Task SeedRolesAndUsersAsync_HandlesMissingUsersSafely()
        {
            var roleManager = MockRoleManager();
            var userManager = MockUserManager();
            var logger = MockLogger();

            roleManager.Setup(x => x.RoleExistsAsync(It.IsAny<string>())).ReturnsAsync(true);

            userManager.Setup(x => x.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((ApplicationUser?)null);

            // Should complete without exceptions
            await IdentityDataSeeder.SeedRolesAndUsersAsync(roleManager.Object, userManager.Object, logger.Object);

            userManager.Verify(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
        }
    }
}
