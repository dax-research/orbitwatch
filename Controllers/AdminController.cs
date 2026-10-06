using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrbitWatch.Models;
using OrbitWatch.Models.Admin;

namespace OrbitWatch.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            var userViewModels = new List<UserViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var currentRole = roles.FirstOrDefault() ?? "None";

                userViewModels.Add(new UserViewModel
                {
                    Id = user.Id,
                    Email = user.Email ?? string.Empty,
                    UserName = user.UserName ?? string.Empty,
                    CurrentRole = currentRole
                });
            }

            return View(userViewModels);
        }

        [HttpGet]
        public async Task<IActionResult> EditRole(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var roles = await _userManager.GetRolesAsync(user);
            var currentRole = roles.FirstOrDefault() ?? string.Empty;

            var allRoles = _roleManager.Roles.Select(r => r.Name).ToList();
            if (allRoles == null) allRoles = new List<string?>();

            var model = new EditRoleViewModel
            {
                UserId = user.Id,
                Email = user.Email ?? string.Empty,
                SelectedRole = currentRole,
                AvailableRoles = allRoles.Where(r => !string.IsNullOrEmpty(r)).Select(r => r!).ToList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditRole(EditRoleViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var allRoles = _roleManager.Roles.Select(r => r.Name).ToList();
                model.AvailableRoles = allRoles.Where(r => !string.IsNullOrEmpty(r)).Select(r => r!).ToList();
                return View(model);
            }

            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null)
            {
                return NotFound();
            }

            // Prevent self-demotion from Admin
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser != null && currentUser.Id == user.Id)
            {
                var isCurrentlyAdmin = await _userManager.IsInRoleAsync(user, "Admin");
                if (isCurrentlyAdmin && model.SelectedRole != "Admin")
                {
                    ModelState.AddModelError(string.Empty, "You cannot remove your own Admin role.");
                    
                    var allRoles = _roleManager.Roles.Select(r => r.Name).ToList();
                    model.AvailableRoles = allRoles.Where(r => !string.IsNullOrEmpty(r)).Select(r => r!).ToList();
                    return View(model);
                }
            }

            // Validate the selected role
            if (!await _roleManager.RoleExistsAsync(model.SelectedRole))
            {
                ModelState.AddModelError(string.Empty, "Invalid role selected.");
                
                var allRoles = _roleManager.Roles.Select(r => r.Name).ToList();
                model.AvailableRoles = allRoles.Where(r => !string.IsNullOrEmpty(r)).Select(r => r!).ToList();
                return View(model);
            }

            var currentRoles = await _userManager.GetRolesAsync(user);
            
            // Remove from existing roles
            var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
            if (!removeResult.Succeeded)
            {
                ModelState.AddModelError(string.Empty, "Failed to remove existing roles.");
                
                var allRoles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
                model.AvailableRoles = allRoles.Where(r => !string.IsNullOrEmpty(r)).Select(r => r!).ToList();
                return View(model);
            }

            // Add new role
            var addResult = await _userManager.AddToRoleAsync(user, model.SelectedRole);
            if (!addResult.Succeeded)
            {
                ModelState.AddModelError(string.Empty, "Failed to assign the new role.");
                
                var allRoles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
                model.AvailableRoles = allRoles.Where(r => !string.IsNullOrEmpty(r)).Select(r => r!).ToList();
                return View(model);
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
