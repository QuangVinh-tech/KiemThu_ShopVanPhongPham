namespace ShopVanPhongPham.Models.Interfaces
{
    public interface IProductRepository
    {
        IEnumerable<Product> GetAllProducts();
        Product? GetProductById(int id);
    }
}
