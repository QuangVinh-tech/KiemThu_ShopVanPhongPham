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
                .Include(o => o.OrderDetails)   // không còn CS8620
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
    }
}
