using Microsoft.EntityFrameworkCore;
using ShopVanPhongPham.Data;
using ShopVanPhongPham.Models.Interfaces;

namespace ShopVanPhongPham.Models.Services
{
    public class WishlistRepository : IWishlistRepository
    {
        private readonly AppDbContext _context;

        public WishlistRepository(AppDbContext context)
        {
            _context = context;
        }

        public void AddToWishlist(string userId, int productId)
        {
            var exists = _context.WishlistItems
                .Any(x => x.UserId == userId && x.ProductId == productId);

            if (!exists)
            {
                _context.WishlistItems.Add(new WishlistItem
                {
                    UserId = userId,
                    ProductId = productId,
                    AddedDate = DateTime.Now
                });
                _context.SaveChanges();
            }
        }

        public void RemoveFromWishlist(string userId, int productId)
        {
            var item = _context.WishlistItems
                .FirstOrDefault(x => x.UserId == userId && x.ProductId == productId);

            if (item != null)
            {
                _context.WishlistItems.Remove(item);
                _context.SaveChanges();
            }
        }

        public List<WishlistItem> GetWishlistItems(string userId)
        {
            return _context.WishlistItems
                .Include(x => x.Product)
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.AddedDate)
                .ToList();
        }

        public int GetWishlistCount(string userId)
        {
            return _context.WishlistItems.Count(x => x.UserId == userId);
        }

        public bool IsInWishlist(string userId, int productId)
        {
            return _context.WishlistItems
                .Any(x => x.UserId == userId && x.ProductId == productId);
        }
    }
}
