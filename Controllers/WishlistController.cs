using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ShopVanPhongPham.Models.Interfaces;

namespace ShopVanPhongPham.Controllers;

public class WishlistController : Controller
{
    private readonly IWishlistRepository _wishlistRepo;
    private readonly UserManager<IdentityUser> _userManager;

    public WishlistController(IWishlistRepository wishlistRepo, UserManager<IdentityUser> userManager)
    {
        _wishlistRepo = wishlistRepo;
        _userManager = userManager;
    }

    // GET /Wishlist
    [Authorize]
    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User)!;
        var items = _wishlistRepo.GetWishlistItems(userId);
        return View(items);
    }

    // POST /Wishlist/Toggle — thêm nếu chưa có, xóa nếu đã có (dùng cho nút trái tim ở product card)
    [HttpPost]
    public IActionResult Toggle(int productId, string? returnUrl)
    {
        if (!User.Identity!.IsAuthenticated)
        {
            TempData["ErrorMessage"] = "Bạn cần đăng nhập để dùng danh sách yêu thích.";
            return RedirectToPage("/Account/Login", new { area = "Identity", returnUrl });
        }

        var userId = _userManager.GetUserId(User)!;

        if (_wishlistRepo.IsInWishlist(userId, productId))
        {
            _wishlistRepo.RemoveFromWishlist(userId, productId);
            TempData["SuccessMessage"] = "Đã bỏ khỏi danh sách yêu thích.";
        }
        else
        {
            _wishlistRepo.AddToWishlist(userId, productId);
            TempData["SuccessMessage"] = "Đã thêm vào danh sách yêu thích!";
        }

        return Redirect(returnUrl ?? Url.Action("Index", "Product")!);
    }

    // POST /Wishlist/Remove — xóa từ chính trang Wishlist
    [Authorize]
    [HttpPost]
    public IActionResult Remove(int productId)
    {
        var userId = _userManager.GetUserId(User)!;
        _wishlistRepo.RemoveFromWishlist(userId, productId);
        TempData["SuccessMessage"] = "Đã xóa khỏi danh sách yêu thích.";
        return RedirectToAction("Index");
    }
}
