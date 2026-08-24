using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopVanPhongPham.Data;
using ShopVanPhongPham.Models;

namespace ShopVanPhongPham.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class NewsController : Controller
    {
        private readonly AppDbContext _context;

        public NewsController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            if (TempData["Success"] != null)
                ViewBag.Success = TempData["Success"];
            return View(_context.News.OrderByDescending(n => n.CreatedAt).ToList());
        }

        public IActionResult Create() => View();

        [HttpPost]
        public IActionResult Create(News news)
        {
            if (!ModelState.IsValid)
                return View(news);

            news.CreatedAt = DateTime.Now;
            _context.News.Add(news);
            _context.SaveChanges();
            TempData["Success"] = $"Đã đăng tin \"{news.Title}\" thành công!";
            return RedirectToAction("Index");
        }

        public IActionResult Edit(int id)
        {
            var news = _context.News.Find(id);
            if (news == null) return NotFound();
            return View(news);
        }

        [HttpPost]
        public IActionResult Edit(News news)
        {
            if (!ModelState.IsValid)
                return View(news);

            _context.News.Update(news);
            _context.SaveChanges();
            TempData["Success"] = $"Đã cập nhật \"{news.Title}\" thành công!";
            return RedirectToAction("Index");
        }

        public IActionResult Delete(int id)
        {
            var news = _context.News.Find(id);
            if (news == null) return NotFound();
            return View(news);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            var news = _context.News.Find(id);
            if (news != null)
            {
                _context.News.Remove(news);
                _context.SaveChanges();
                TempData["Success"] = $"Đã xóa \"{news.Title}\"!";
            }
            return RedirectToAction("Index");
        }
    }
}