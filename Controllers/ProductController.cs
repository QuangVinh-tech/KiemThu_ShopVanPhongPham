using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ShopVanPhongPham.Helpers;
using ShopVanPhongPham.Models.Interfaces;
using ShopVanPhongPham.Data;

namespace ShopVanPhongPham.Controllers;

public class ProductController : Controller
{
    private readonly IProductRepository _productRepo;
    private readonly AppDbContext _context;
    private readonly IWishlistRepository _wishlistRepo;
    private readonly UserManager<IdentityUser> _userManager;

    public ProductController(IProductRepository productRepo, AppDbContext context,
                              IWishlistRepository wishlistRepo, UserManager<IdentityUser> userManager)
    {
        _productRepo = productRepo;
        _context = context;
        _wishlistRepo = wishlistRepo;
        _userManager = userManager;
    }
    public IActionResult Shop(string? search, string? category, decimal? minPrice, decimal? maxPrice, string? sort)
    {
        var allProducts = _productRepo.GetAllProducts();

        var categories = _context.Categories
            .OrderBy(c => c.Name)
            .Select(c => c.Name)
            .ToList();

        var products = allProducts.AsEnumerable();

        if (!string.IsNullOrEmpty(search))
        {
            var keyword = StringHelper.RemoveDiacritics(search);
            products = products.Where(p =>
                StringHelper.RemoveDiacritics(p.Name).Contains(keyword));
        }

        if (!string.IsNullOrEmpty(category))
            products = products.Where(p => p.Category != null && p.Category.Name == category);

        if (minPrice.HasValue)
            products = products.Where(p => p.Price >= minPrice.Value);

        if (maxPrice.HasValue)
            products = products.Where(p => p.Price <= maxPrice.Value);

        products = sort switch
        {
            "price_asc" => products.OrderBy(p => p.Price),
            "price_desc" => products.OrderByDescending(p => p.Price),
            "name_asc" => products.OrderBy(p => p.Name),
            "name_desc" => products.OrderByDescending(p => p.Name),
            _ => products
        };

        ViewBag.Search = search;
        ViewBag.Category = category;
        ViewBag.Categories = categories;
        ViewBag.MinPrice = minPrice;
        ViewBag.MaxPrice = maxPrice;
        ViewBag.Sort = sort;

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

    public IActionResult Detail(int id)
    {
        var product = _productRepo.GetProductById(id);
        if (product == null) return NotFound();

        if (User.Identity!.IsAuthenticated)
        {
            var userId = _userManager.GetUserId(User)!;
            ViewBag.IsWished = _wishlistRepo.IsInWishlist(userId, id);
        }
        else
        {
            ViewBag.IsWished = false;
        }

        return View(product);
    }
}