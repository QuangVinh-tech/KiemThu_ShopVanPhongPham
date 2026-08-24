namespace ShopVanPhongPham.Models.Interfaces
{
    public interface IWishlistRepository
    {
        void AddToWishlist(string userId, int productId);
        void RemoveFromWishlist(string userId, int productId);
        List<WishlistItem> GetWishlistItems(string userId);
        int GetWishlistCount(string userId);
        bool IsInWishlist(string userId, int productId);
    }
}
