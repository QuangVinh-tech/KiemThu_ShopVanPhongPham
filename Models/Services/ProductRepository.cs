using ShopVanPhongPham.Data;
using ShopVanPhongPham.Models.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ShopVanPhongPham.Models.Services
{
    public class ProductRepository : IProductRepository
    {
        private readonly AppDbContext _context;
        public ProductRepository(AppDbContext context)
        {
            _context = context;
        }
        public IEnumerable<Product> GetAllProducts()
        {
            return _context.Products
                .Include(p => p.Category)
                .ToList();
        }
        public Product? GetProductById(int id)
        {
            return _context.Products
                .Include(p => p.Category)
                .FirstOrDefault(p => p.Id == id);
        }
    }
}