using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopVanPhongPham.Data;
using ShopVanPhongPham.Models;

namespace ShopVanPhongPham.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class PromotionController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public PromotionController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public IActionResult Index()
        {
            if (TempData["Success"] != null)
                ViewBag.Success = TempData["Success"];

            var promotions = _context.Promotions
                .Include(p => p.Product)
                .OrderByDescending(p => p.Id)
                .ToList();

            return View(promotions);
        }

        public IActionResult Create()
        {
            ViewBag.Products = _context.Products.OrderBy(p => p.Name).ToList();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Promotion promotion, IFormFile? imageFile)
        {
            if (imageFile != null && imageFile.Length > 0)
            {
                var fileName = Guid.NewGuid() + Path.GetExtension(imageFile.FileName);
                var dir = Path.Combine(_env.WebRootPath, "assets", "images", "promotions");
                Directory.CreateDirectory(dir);
                var savePath = Path.Combine(dir, fileName);
                using var stream = new FileStream(savePath, FileMode.Create);
                await imageFile.CopyToAsync(stream);
                promotion.ImageUrl = "/assets/images/promotions/" + fileName;
            }
            else
            {
                promotion.ImageUrl = "/assets/images/promo-default.jpg";
            }

            ModelState.Remove("ImageUrl");
            ModelState.Remove("Product");

            if (promotion.ProductId == 0) promotion.ProductId = null;

            if (promotion.EndDate < promotion.StartDate)
                ModelState.AddModelError("EndDate", "Ngày kết thúc phải sau ngày bắt đầu.");

            if (!string.IsNullOrWhiteSpace(promotion.Code) &&
                _context.Promotions.Any(p => p.Code == promotion.Code))
                ModelState.AddModelError("Code", "Mã này đã tồn tại, vui lòng chọn mã khác.");

            if (!ModelState.IsValid)
            {
                ViewBag.Products = _context.Products.OrderBy(p => p.Name).ToList();
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                ViewBag.DebugErrors = string.Join(" | ", errors);
                return View(promotion);
            }

            _context.Promotions.Add(promotion);
            _context.SaveChanges();
            TempData["Success"] = $"Đã thêm khuyến mãi \"{promotion.Title}\" thành công!";
            return RedirectToAction("Index");
        }

        public IActionResult Edit(int id)
        {
            var promotion = _context.Promotions.Find(id);
            if (promotion == null) return NotFound();
            ViewBag.Products = _context.Products.OrderBy(p => p.Name).ToList();
            return View(promotion);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Promotion promotion, IFormFile? imageFile)
        {
            if (imageFile != null && imageFile.Length > 0)
            {
                var fileName = Guid.NewGuid() + Path.GetExtension(imageFile.FileName);
                var dir = Path.Combine(_env.WebRootPath, "assets", "images", "promotions");
                Directory.CreateDirectory(dir);
                var savePath = Path.Combine(dir, fileName);
                using var stream = new FileStream(savePath, FileMode.Create);
                await imageFile.CopyToAsync(stream);
                promotion.ImageUrl = "/assets/images/promotions/" + fileName;
            }

            ModelState.Remove("ImageUrl");
            ModelState.Remove("Product");

            if (promotion.ProductId == 0) promotion.ProductId = null;

            if (promotion.EndDate < promotion.StartDate)
                ModelState.AddModelError("EndDate", "Ngày kết thúc phải sau ngày bắt đầu.");

            if (!string.IsNullOrWhiteSpace(promotion.Code) &&
                _context.Promotions.Any(p => p.Code == promotion.Code && p.Id != promotion.Id))
                ModelState.AddModelError("Code", "Mã này đã tồn tại, vui lòng chọn mã khác.");

            if (!ModelState.IsValid)
            {
                ViewBag.Products = _context.Products.OrderBy(p => p.Name).ToList();
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                ViewBag.DebugErrors = string.Join(" | ", errors);
                return View(promotion);
            }

            _context.Promotions.Update(promotion);
            _context.SaveChanges();
            TempData["Success"] = $"Đã cập nhật \"{promotion.Title}\" thành công!";
            return RedirectToAction("Index");
        }

        public IActionResult Delete(int id)
        {
            var promotion = _context.Promotions.Include(p => p.Product).FirstOrDefault(p => p.Id == id);
            if (promotion == null) return NotFound();
            return View(promotion);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            var promotion = _context.Promotions.Find(id);
            if (promotion != null)
            {
                _context.Promotions.Remove(promotion);
                _context.SaveChanges();
                TempData["Success"] = $"Đã xóa \"{promotion.Title}\"!";
            }
            return RedirectToAction("Index");
        }
    }
}
