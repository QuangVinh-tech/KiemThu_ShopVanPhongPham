using Microsoft.AspNetCore.Mvc;
using ShopVanPhongPham.Data;

namespace ShopVanPhongPham.Controllers
{
    public class NewsController : Controller
    {
        private readonly AppDbContext _context;

        public NewsController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var newsList = _context.News.OrderByDescending(n => n.CreatedAt).ToList();
            return View(newsList);
        }

        public IActionResult Detail(int id)
        {
            var news = _context.News.Find(id);
            if (news == null) return NotFound();
            return View(news);
        }
    }
}