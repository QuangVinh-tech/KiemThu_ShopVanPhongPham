using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopVanPhongPham.Data;
using ShopVanPhongPham.Models;
using ShopVanPhongPham.Models.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ShopVanPhongPham.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class OrderController : Controller
    {
        private readonly IOrderRepository _orderRepo;
        private readonly AppDbContext _context;

        public OrderController(IOrderRepository orderRepo, AppDbContext context)
        {
            _orderRepo = orderRepo;
            _context = context;
        }

        public IActionResult Index(string? status)
        {
            var orders = _orderRepo.GetAllOrders();

            if (!string.IsNullOrEmpty(status))
            {
                // "Chờ xử lý" khớp cả null lẫn chuỗi "Chờ xử lý"
                if (status == "Chờ xử lý")
                    orders = orders
                        .Where(o => string.IsNullOrEmpty(o.Status) || o.Status == "Chờ xử lý")
                        .ToList();
                else
                    orders = orders.Where(o => o.Status == status).ToList();
            }

            ViewBag.StatusFilter = status;
            ViewBag.StatusList = new List<string> { "Chờ xử lý", "Đang giao", "Đã giao", "Đã hủy" };
            ViewBag.TotalOrders = _context.Orders.Count();
            ViewBag.PendingOrders = _context.Orders.Count(o => string.IsNullOrEmpty(o.Status) || o.Status == "Chờ xử lý");
            ViewBag.ShippingOrders = _context.Orders.Count(o => o.Status == "Đang giao");
            ViewBag.DoneOrders = _context.Orders.Count(o => o.Status == "Đã giao");
            ViewBag.CancelledOrders = _context.Orders.Count(o => o.Status == "Đã hủy");

            return View(orders);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateStatus(int orderId, string status, string? filter, int? returnId)
        {
            var order = _context.Orders.Find(orderId);
            if (order == null) return NotFound();
            order.Status = status;
            _context.SaveChanges();
            TempData["Success"] = $"Đã cập nhật đơn #{orderId} → {status}";

            if (returnId.HasValue)
                return RedirectToAction("Detail", new { id = returnId.Value });

            return RedirectToAction("Index", new { status = filter });
        }

        public IActionResult Detail(int id)
        {
            var order = _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .FirstOrDefault(o => o.Id == id);
            if (order == null) return NotFound();
            return View(order);
        }
    }
}
