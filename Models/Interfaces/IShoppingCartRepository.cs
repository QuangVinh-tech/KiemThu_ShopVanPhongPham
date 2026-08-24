namespace ShopVanPhongPham.Models.Interfaces
{
    public interface IShoppingCartRepository
    {
        void AddToCart(int productId);
        void AddToCart(Product product, int quantity);  
        void RemoveFromCart(int id);
        List<ShoppingCartItem> GetCartItems();
        decimal GetCartTotal();                        
        int GetCartCount();
        void IncreaseQuantity(int productId);           
        void DecreaseQuantity(int productId);           
        void ClearCart();
    }

}
