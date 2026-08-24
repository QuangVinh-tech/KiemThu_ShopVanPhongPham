using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ShopVanPhongPham.Data;

namespace ShopVanPhongPham.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public AdminController(AppDbContext context,
                               UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            ViewBag.TotalProducts = _context.Products.Count();
            ViewBag.TotalOrders = _context.Orders.Count();
            ViewBag.TotalUsers = _userManager.Users.Count();
            ViewBag.TotalRevenue = _context.Orders.Sum(o => (decimal?)o.OrderTotal) ?? 0;

            // Phân loại đơn hàng
            ViewBag.PendingOrders = _context.Orders.Count(o => o.Status == null || o.Status == "Chờ xử lý");
            ViewBag.ShippingOrders = _context.Orders.Count(o => o.Status == "Đang giao");
            ViewBag.DoneOrders = _context.Orders.Count(o => o.Status == "Đã giao");
            ViewBag.CancelledOrders = _context.Orders.Count(o => o.Status == "Đã hủy");

            return View();
        }
    }
}
