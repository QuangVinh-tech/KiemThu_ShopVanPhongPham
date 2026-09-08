using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
    private readonly IEmailService _emailService;

    public HomeController(IProductRepository productRepo, AppDbContext context,
                           IWishlistRepository wishlistRepo, UserManager<IdentityUser> userManager,
                           IEmailService emailService)
    {
        _productRepo = productRepo;
        _context = context;
        _wishlistRepo = wishlistRepo;
        _userManager = userManager;
        _emailService = emailService;
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

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Subscribe(string email)
    {
        if (string.IsNullOrWhiteSpace(email) || !new System.ComponentModel.DataAnnotations.EmailAddressAttribute().IsValid(email))
        {
            return Json(new { success = false, message = "Vui lòng nhập email hợp lệ." });
        }

        bool alreadyExists = await _context.NewsletterSubscribers
            .AnyAsync(s => s.Email == email);

        if (alreadyExists)
        {
            return Json(new { success = false, message = "Email này đã đăng ký nhận tin trước đó." });
        }

        _context.NewsletterSubscribers.Add(new NewsletterSubscriber
        {
            Email = email,
            SubscribedAt = DateTime.Now
        });
        await _context.SaveChangesAsync();

        var (sent, sendMessage) = await _emailService.SendEmailAsync(
            email,
            "Cảm ơn bạn đã đăng ký nhận tin từ VanPhamPro Shop",
            "<p>Xin chào,</p><p>Cảm ơn bạn đã đăng ký nhận tin khuyến mãi từ <strong>VanPhamPro Shop</strong>. " +
            "Bạn sẽ là người đầu tiên biết về các ưu đãi và sản phẩm mới!</p>");

        if (!sent)
        {
            // Đã lưu email vào DB thành công, chỉ gửi mail xác nhận thất bại (không chặn đăng ký)
            return Json(new { success = true, message = "Đăng ký thành công! (Không gửi được email xác nhận, vui lòng kiểm tra cấu hình SMTP)" });
        }

        return Json(new { success = true, message = "Đăng ký thành công! Vui lòng kiểm tra hộp thư của bạn." });
    }

    public IActionResult TuuTruong()
    {
        // Danh mục thuộc chủ đề "Mùa tựu trường": Bút, Sổ, Dụng cụ học tập, Giấy in
        var schoolCategories = new[] { "But", "So", "DungCu", "Giay" };

        var products = _productRepo.GetAllProducts()
            .Where(p => p.Category != null && schoolCategories.Contains(p.Category.Name))
            .OrderBy(p => p.Price)
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

        return View(products);
    }
}