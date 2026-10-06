using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using OrbitWatch.Controllers;
using OrbitWatch.Models;
using OrbitWatch.Models.Admin;
using System.Security.Claims;
using Xunit;

namespace OrbitWatch.Tests
{
    public class AdminControllerTests
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

        [Fact]
        public async Task EditRole_Get_ReturnsNotFound_IfUserDoesNotExist()
        {
            var userManager = MockUserManager();
            var roleManager = MockRoleManager();

            userManager.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync((ApplicationUser?)null);

            var controller = new AdminController(userManager.Object, roleManager.Object);

            var result = await controller.EditRole("invalid-id");

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task EditRole_Post_ReturnsNotFound_IfUserDoesNotExist()
        {
            var userManager = MockUserManager();
            var roleManager = MockRoleManager();

            userManager.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync((ApplicationUser?)null);

            var controller = new AdminController(userManager.Object, roleManager.Object);
            var model = new EditRoleViewModel { UserId = "invalid-id", SelectedRole = "Admin" };

            var result = await controller.EditRole(model);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task EditRole_Post_PreventsSelfDemotion()
        {
            var userManager = MockUserManager();
            var roleManager = MockRoleManager();

            var adminUser = new ApplicationUser { Id = "admin-1", UserName = "admin" };

            userManager.Setup(x => x.FindByIdAsync("admin-1")).ReturnsAsync(adminUser);
            userManager.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(adminUser);
            userManager.Setup(x => x.IsInRoleAsync(adminUser, "Admin")).ReturnsAsync(true);
            
            var rolesList = new List<IdentityRole>
            {
                new IdentityRole("Admin"),
                new IdentityRole("Manager"),
                new IdentityRole("Analyst")
            };
            roleManager.Setup(r => r.Roles).Returns(rolesList.AsQueryable());

            var controller = new AdminController(userManager.Object, roleManager.Object);
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] {
                new Claim(ClaimTypes.NameIdentifier, "admin-1")
            }, "TestAuthentication"));
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            var model = new EditRoleViewModel { UserId = "admin-1", SelectedRole = "Analyst" };

            var result = await controller.EditRole(model);
                
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(controller.ModelState.IsValid);
            Assert.True(controller.ModelState.ErrorCount > 0);
        }

        [Fact]
        public async Task EditRole_Post_AssignsNewRoleAndRemovesOldRoles()
        {
            var userManager = MockUserManager();
            var roleManager = MockRoleManager();

            var targetUser = new ApplicationUser { Id = "user-1", UserName = "user" };

            userManager.Setup(x => x.FindByIdAsync("user-1")).ReturnsAsync(targetUser);
            userManager.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(new ApplicationUser { Id = "admin-1" });
            
            userManager.Setup(x => x.GetRolesAsync(targetUser)).ReturnsAsync(new List<string> { "Analyst" });
            roleManager.Setup(x => x.RoleExistsAsync("Manager")).ReturnsAsync(true);
            
            userManager.Setup(x => x.RemoveFromRolesAsync(targetUser, It.IsAny<IEnumerable<string>>())).ReturnsAsync(IdentityResult.Success);
            userManager.Setup(x => x.AddToRoleAsync(targetUser, "Manager")).ReturnsAsync(IdentityResult.Success);

            var rolesList = new List<IdentityRole>
            {
                new IdentityRole("Admin"),
                new IdentityRole("Manager"),
                new IdentityRole("Analyst")
            };
            roleManager.Setup(r => r.Roles).Returns(rolesList.AsQueryable());

            var controller = new AdminController(userManager.Object, roleManager.Object);
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] {
                new Claim(ClaimTypes.NameIdentifier, "admin-1")
            }, "TestAuthentication"));
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            var model = new EditRoleViewModel { UserId = "user-1", SelectedRole = "Manager" };

            var result = await controller.EditRole(model);
                
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            
            userManager.Verify(x => x.RemoveFromRolesAsync(targetUser, It.Is<IEnumerable<string>>(r => r.Contains("Analyst"))), Times.Once);
            userManager.Verify(x => x.AddToRoleAsync(targetUser, "Manager"), Times.Once);
        }
    }
}
