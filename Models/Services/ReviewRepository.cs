using Microsoft.EntityFrameworkCore;
using ShopVanPhongPham.Data;
using ShopVanPhongPham.Models.Interfaces;

namespace ShopVanPhongPham.Models.Services
{
    public class ReviewRepository : IReviewRepository
    {
        private readonly AppDbContext _context;

        public ReviewRepository(AppDbContext context)
        {
            _context = context;
        }

        public List<Review> GetReviewsByProduct(int productId)
        {
            return _context.Reviews
                .Include(r => r.User)
                .Where(r => r.ProductId == productId)
                .OrderByDescending(r => r.CreatedAt)
                .ToList();
        }

        public double GetAverageRating(int productId)
        {
            var reviews = _context.Reviews.Where(r => r.ProductId == productId);
            return reviews.Any() ? reviews.Average(r => r.Rating) : 0;
        }

        public int GetReviewCount(int productId)
        {
            return _context.Reviews.Count(r => r.ProductId == productId);
        }

        public bool CanReview(string userId, int productId, out int orderId)
        {
            orderId = 0;

            var user = _context.Users.Find(userId);
            if (user?.Email == null) return false;

            var order = _context.Orders
                .Include(o => o.OrderDetails)
                .Where(o => o.Email == user.Email
                         && o.Status == "Đã giao"
                         && o.OrderDetails.Any(od => od.ProductId == productId))
                .OrderByDescending(o => o.OrderPlaced)
                .FirstOrDefault(o => !_context.Reviews.Any(r =>
                    r.OrderId == o.Id && r.ProductId == productId && r.UserId == userId));

            if (order == null) return false;

            orderId = order.Id;
            return true;
        }

        public bool HasReviewed(string userId, int productId, int orderId)
        {
            return _context.Reviews.Any(r =>
                r.OrderId == orderId && r.ProductId == productId && r.UserId == userId);
        }

        public Review AddReview(Review review)
        {
            _context.Reviews.Add(review);
            _context.SaveChanges();
            return review;
        }
    }
}