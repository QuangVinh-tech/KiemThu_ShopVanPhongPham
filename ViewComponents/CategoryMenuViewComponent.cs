using Microsoft.AspNetCore.Mvc;
using ShopVanPhongPham.Data;

namespace ShopVanPhongPham.ViewComponents
{
    public class CategoryMenuViewComponent : ViewComponent
    {
        private readonly AppDbContext _context;

        public CategoryMenuViewComponent(AppDbContext context)
        {
            _context = context;
        }

        public IViewComponentResult Invoke()
        {
            var categories = _context.Categories
                .OrderBy(c => c.Name)
                .ToList();

            return View(categories);
        }
    }
}