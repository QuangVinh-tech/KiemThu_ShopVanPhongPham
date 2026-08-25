using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ShopVanPhongPham.Data;
using ShopVanPhongPham.Models;
using ShopVanPhongPham.Models.Interfaces;

namespace ShopVanPhongPham.Controllers;

public class HomeController : Controller
{
    private readonly IProductRepository _productRepo;
    private readonly AppDbContext _context;
    private readonly IWishlistRepository _wishlistRepo;
    private readonly UserManager<IdentityUser> _userManager;

    public HomeController(IProductRepository productRepo, AppDbContext context,
                           IWishlistRepository wishlistRepo, UserManager<IdentityUser> userManager)
    {
        _productRepo = productRepo;
        _context = context;
        _wishlistRepo = wishlistRepo;
        _userManager = userManager;
    }

    public IActionResult Index()
    {
        var products = _productRepo.GetAllProducts();
        ViewBag.Categories = products
            .Where(p => p.Category != null)
            .GroupBy(p => p.Category!.Name)
            .Select(g => new { Name = g.Key, Count = g.Count() })
            .OrderBy(c => c.Name)
            .ToList();

        if (User.Identity!.IsAuthenticated)
        {
            var userId = _userManager.GetUserId(User)!;
            ViewBag.WishlistIds = _wishlistRepo.GetWishlistItems(userId)
                .Select(w => w.ProductId).ToHashSet();
        }
        else
        {
            ViewBag.WishlistIds = new HashSet<int>();
        }

        return View(products.ToList());
    }

    public IActionResult About() => View();

    public IActionResult Contact() => View();

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Contact(
        string fullName,
        string email,
        string phone,
        string subject,
        string message)
    {
        if (string.IsNullOrWhiteSpace(fullName) ||
            string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(message))
        {
            TempData["ErrorMessage"] = "Vui lòng điền đầy đủ thông tin bắt buộc.";
            return RedirectToAction("Contact");
        }
        var contactMessage = new ContactMessage
        {
            FullName = fullName,
            Email = email,
            Phone = phone,
            Subject = string.IsNullOrWhiteSpace(subject) ? "Khác" : subject,
            Message = message,
            SentAt = DateTime.Now,
            IsRead = false
        };
        _context.ContactMessages.Add(contactMessage);
        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] =
            "Cảm ơn bạn đã liên hệ! Chúng tôi sẽ phản hồi sớm nhất có thể.";
        return RedirectToAction("Contact");
    }
}