using Microsoft.EntityFrameworkCore;
using ShopVanPhongPham.Data;
using ShopVanPhongPham.Models.Interfaces;
namespace ShopVanPhongPham.Models.Services
{
    public class OrderRepository : IOrderRepository
    {
        private readonly AppDbContext _context;
        private readonly IShoppingCartRepository _cart;
        public OrderRepository(AppDbContext context, IShoppingCartRepository cart)
        {
            _context = context;
            _cart = cart;
        }
        public Order PlaceOrder(Order order)
        {
            if (order.OrderPlaced == default)
                order.OrderPlaced = DateTime.Now;
            if (order.OrderTotal == 0)
                order.OrderTotal = _cart.GetCartTotal();
            _context.Orders.Add(order);
            _context.SaveChanges();
            return order;
        }
        public List<Order> GetAllOrders()
        {
            return _context.Orders
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .OrderByDescending(o => o.OrderPlaced)
                .ToList();
        }
        public List<Order> GetOrdersByEmail(string email)
        {
            return _context.Orders
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .Where(o => o.Email == email)
                .OrderByDescending(o => o.OrderPlaced)
                .ToList();
        }
        public Order? GetOrderById(int id)
        {
            return _context.Orders
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .FirstOrDefault(o => o.Id == id);
        }
        public void UpdateOrder(Order order)
        {
            _context.Orders.Update(order);
            _context.SaveChanges();
        }

        // Các trạng thái đơn hàng hợp lệ (khớp với Areas/Admin/Controllers/OrderController.cs)
        private const string STATUS_PENDING = "Chờ xử lý";
        private const string STATUS_CANCELLED = "Đã hủy";

        public (bool Success, string Message) CancelOrder(int orderId, string userEmail)
        {
            var order = _context.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefault(o => o.Id == orderId);

            if (order == null)
                return (false, "Không tìm thấy đơn hàng.");

            // Chống IDOR: chỉ chủ đơn hàng (đúng email) mới được hủy
            if (string.IsNullOrEmpty(userEmail) ||
                !string.Equals(order.Email, userEmail, StringComparison.OrdinalIgnoreCase))
            {
                return (false, "Bạn không có quyền hủy đơn hàng này.");
            }

            var currentStatus = string.IsNullOrEmpty(order.Status) ? STATUS_PENDING : order.Status;

            if (currentStatus == STATUS_CANCELLED)
                return (false, "Đơn hàng này đã được hủy trước đó.");

            if (currentStatus != STATUS_PENDING)
                return (false, $"Không thể hủy đơn hàng đang ở trạng thái \"{currentStatus}\". " +
                                 "Vui lòng liên hệ shop để được hỗ trợ.");

            order.Status = STATUS_CANCELLED;

            // Hoàn trả số lượng sản phẩm về kho — cùng logic với MomoReturn khi thanh toán thất bại
            foreach (var detail in order.OrderDetails)
            {
                var product = _context.Products.Find(detail.ProductId);
                if (product != null)
                {
                    product.Stock += detail.Quantity;
                }
            }

            _context.SaveChanges();
            return (true, $"Đã hủy đơn hàng #{order.Id} thành công.");
        }
    }
}