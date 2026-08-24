using Microsoft.EntityFrameworkCore;
using ShopVanPhongPham.Data;
using ShopVanPhongPham.Models.Interfaces;

namespace ShopVanPhongPham.Models.Services
{
    public class ShoppingCartRepository : IShoppingCartRepository
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private string CartId;

        public ShoppingCartRepository(AppDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            CartId = GetCartId();
        }

        private string GetCartId()
        {
            var session = _httpContextAccessor.HttpContext!.Session;
            string? cartId = session.GetString("CartId");
            if (cartId == null)
            {
                cartId = Guid.NewGuid().ToString();
                session.SetString("CartId", cartId);
            }
            return cartId;
        }

        private void UpdateCartCount()
        {
            var count = GetCartCount();
            _httpContextAccessor.HttpContext!.Session.SetInt32("CartCount", count);
        }

        // ── AddToCart overload 1: nhận productId ─────────────────────────
        public void AddToCart(int productId)
        {
            var product = _context.Products.Find(productId);
            if (product == null) return;
            AddToCart(product, 1);
        }

        // ── AddToCart overload 2: nhận Product + quantity ─────────────────
        public void AddToCart(Product product, int quantity)
        {
            var item = _context.ShoppingCartItems
                .FirstOrDefault(x => x.CartId == CartId && x.ProductId == product.Id);

            if (item == null)
            {
                _context.ShoppingCartItems.Add(new ShoppingCartItem
                {
                    CartId = CartId,
                    ProductId = product.Id,
                    Quantity = quantity
                });
            }
            else
            {
                item.Quantity += quantity;
            }

            _context.SaveChanges();
            UpdateCartCount();
        }

        // ── Xóa theo Id của ShoppingCartItem ─────────────────────────────
        public void RemoveFromCart(int id)
        {
            var item = _context.ShoppingCartItems.Find(id);
            if (item != null)
            {
                _context.ShoppingCartItems.Remove(item);
                _context.SaveChanges();
                UpdateCartCount();
            }
        }

        // ── Lấy danh sách giỏ hàng ───────────────────────────────────────
        public List<ShoppingCartItem> GetCartItems()
        {
            return _context.ShoppingCartItems
                .Include(x => x.Product)
                .Where(x => x.CartId == CartId)
                .ToList();
        }

        // ── Tổng số lượng (badge navbar) ─────────────────────────────────
        public int GetCartCount()
        {
            return _context.ShoppingCartItems
                .Where(x => x.CartId == CartId)
                .Sum(x => (int?)x.Quantity) ?? 0;
        }

        // ── Tổng tiền ────────────────────────────────────────────────────
        public decimal GetCartTotal()
        {
            return _context.ShoppingCartItems
                .Include(x => x.Product)
                .Where(x => x.CartId == CartId)
                .Sum(x => (decimal?)(x.Product!.Price * x.Quantity)) ?? 0;
        }

        // ── Tăng số lượng ────────────────────────────────────────────────
        public void IncreaseQuantity(int productId)
        {
            var item = _context.ShoppingCartItems
                .FirstOrDefault(x => x.CartId == CartId && x.ProductId == productId);
            if (item != null)
            {
                item.Quantity++;
                _context.SaveChanges();
                UpdateCartCount();
            }
        }

        // ── Giảm số lượng, nếu = 1 thì xóa ──────────────────────────────
        public void DecreaseQuantity(int productId)
        {
            var item = _context.ShoppingCartItems
                .FirstOrDefault(x => x.CartId == CartId && x.ProductId == productId);
            if (item != null)
            {
                if (item.Quantity > 1)
                    item.Quantity--;
                else
                    _context.ShoppingCartItems.Remove(item);

                _context.SaveChanges();
                UpdateCartCount();
            }
        }

        // ── Xóa toàn bộ giỏ ─────────────────────────────────────────────
        public void ClearCart()
        {
            var items = _context.ShoppingCartItems
                .Where(x => x.CartId == CartId)
                .ToList();

            _context.ShoppingCartItems.RemoveRange(items);
            _context.SaveChanges();
            UpdateCartCount();
        }
    }
}