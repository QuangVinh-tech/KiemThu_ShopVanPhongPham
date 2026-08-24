using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering; 
using Microsoft.EntityFrameworkCore;
using ShopVanPhongPham.Data;
using ShopVanPhongPham.Models;

namespace ShopVanPhongPham.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ProductController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ProductController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public IActionResult Index()
        {
            if (TempData["Success"] != null)
                ViewBag.Success = TempData["Success"];
            return View(_context.Products.Include(p => p.Category).ToList());  
        }

        public IActionResult Create()
        {
            ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name");   
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Product product, IFormFile? imageFile)
        {
            if (imageFile != null && imageFile.Length > 0)
            {
                var fileName = Guid.NewGuid() + Path.GetExtension(imageFile.FileName);
                var dir = Path.Combine(_env.WebRootPath, "assets", "images");
                Directory.CreateDirectory(dir);
                var savePath = Path.Combine(dir, fileName);
                using var stream = new FileStream(savePath, FileMode.Create);
                await imageFile.CopyToAsync(stream);
                product.ImageUrl = "/assets/images/" + fileName;
            }
            else
            {
                product.ImageUrl = "/assets/images/hopbut.jpg";
            }

            ModelState.Remove("ImageUrl");
            ModelState.Remove("Category"); 
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                ViewBag.DebugErrors = string.Join(" | ", errors);
                ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);   
                return View(product);
            }

            _context.Products.Add(product);
            _context.SaveChanges();
            TempData["Success"] = $"Đã thêm \"{product.Name}\" thành công!";
            return RedirectToAction("Index");
        }

        public IActionResult Edit(int id)
        {
            var product = _context.Products.Find(id);
            if (product == null) return NotFound();
            ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);  
            return View(product);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Product product, IFormFile? imageFile)
        {
            if (imageFile != null && imageFile.Length > 0)
            {
                var fileName = Guid.NewGuid() + Path.GetExtension(imageFile.FileName);
                var dir = Path.Combine(_env.WebRootPath, "assets", "images");
                Directory.CreateDirectory(dir);
                var savePath = Path.Combine(dir, fileName);
                using var stream = new FileStream(savePath, FileMode.Create);
                await imageFile.CopyToAsync(stream);
                product.ImageUrl = "/assets/images/" + fileName;
            }

            ModelState.Remove("ImageUrl");
            ModelState.Remove("Category");   

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                ViewBag.DebugErrors = string.Join(" | ", errors);
                ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);   
            }

            _context.Products.Update(product);
            _context.SaveChanges();
            TempData["Success"] = $"Đã cập nhật \"{product.Name}\" thành công!";
            return RedirectToAction("Index");
        }

        public IActionResult Delete(int id)
        {
            var product = _context.Products.Find(id);
            if (product == null) return NotFound();
            return View(product);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            var product = _context.Products.Find(id);
            if (product != null)
            {
                _context.Products.Remove(product);
                _context.SaveChanges();
                TempData["Success"] = $"Đã xóa \"{product.Name}\"!";
            }
            return RedirectToAction("Index");
        }
    }
}