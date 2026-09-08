namespace ShopVanPhongPham.Models.Interfaces
{
    public interface IReviewRepository
    {
        List<Review> GetReviewsByProduct(int productId);
        double GetAverageRating(int productId);
        int GetReviewCount(int productId);

     
        bool CanReview(string userId, int productId, out int orderId);

        bool HasReviewed(string userId, int productId, int orderId);

        Review AddReview(Review review);
    }
}