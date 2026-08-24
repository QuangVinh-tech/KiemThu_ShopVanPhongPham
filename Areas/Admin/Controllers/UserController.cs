using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ShopVanPhongPham.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class UserController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserController(UserManager<IdentityUser> userManager,
                              RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index()
        {
            var users = _userManager.Users.ToList();
            var userRoles = new Dictionary<string, IList<string>>();
            foreach (var u in users)
                userRoles[u.Id] = await _userManager.GetRolesAsync(u);

            ViewBag.UserRoles = userRoles;
            if (TempData["Success"] != null)
                ViewBag.Success = TempData["Success"];
            if (TempData["Error"] != null)
                ViewBag.Error = TempData["Error"];

            return View(users);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]   // ← thiếu cái này nên form bị reject
        public async Task<IActionResult> AssignRole(string userId, string role)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            // Đảm bảo role tồn tại trong DB
            if (!await _roleManager.RoleExistsAsync(role))
            {
                TempData["Error"] = $"Role \"{role}\" không tồn tại.";
                return RedirectToAction("Index");
            }

            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            var result = await _userManager.AddToRoleAsync(user, role);

            if (result.Succeeded)
                TempData["Success"] = $"✅ Đã gán role \"{role}\" cho {user.Email}";
            else
                TempData["Error"] = "Gán role thất bại: " + string.Join(", ", result.Errors.Select(e => e.Description));

            return RedirectToAction("Index");
        }
    }
}
