using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SportsShoesEcommerce.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SportsShoesEcommerce.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UsersController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index(string selectedUserId, string selectedRole)
        {
            // 1. Fetch all users from the identity framework
            var usersList = await _userManager.Users.ToListAsync();
            var filteredUsers = new List<ApplicationUser>();

            // 2. Loop through and filter the users list
            foreach (var user in usersList)
            {
                // Filter by chosen User Dropdown selection
                bool matchesUser = string.IsNullOrEmpty(selectedUserId) || user.Id == selectedUserId;

                if (matchesUser)
                {
                    // Filter by chosen Role Dropdown selection
                    if (!string.IsNullOrEmpty(selectedRole))
                    {
                        if (await _userManager.IsInRoleAsync(user, selectedRole))
                        {
                            filteredUsers.Add(user);
                        }
                    }
                    else
                    {
                        filteredUsers.Add(user);
                    }
                }
            }

            // 3. Build the User Dropdown Selection List (Sorted alphabetically)
            var sortedUsersForDropdown = usersList.OrderBy(u => u.FullName).ToList();
            ViewBag.UsersDropdown = new SelectList(sortedUsersForDropdown, "Id", "FullName", selectedUserId);

            // 4. Build the Roles Dropdown Selection List
            var roles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
            ViewBag.RolesList = roles;

            // Retain active tracking state values
            ViewData["CurrentSelectedUserId"] = selectedUserId;
            ViewData["CurrentSelectedRole"] = selectedRole;

            // Build dictionary to display matched user roles on the index grid
            var userRoles = new Dictionary<string, string>();
            foreach (var u in filteredUsers)
            {
                var r = await _userManager.GetRolesAsync(u);
                userRoles[u.Id] = r.FirstOrDefault() ?? "No Role";
            }
            ViewBag.UserRoles = userRoles;

            return View(filteredUsers);
        }
    }
}