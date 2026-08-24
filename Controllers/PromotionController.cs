using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopVanPhongPham.Data;

namespace ShopVanPhongPham.Controllers;

public class PromotionController : Controller
{
    private readonly AppDbContext _context;

    public PromotionController(AppDbContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        var today = DateTime.Today;
        var promotions = _context.Promotions
            .Include(p => p.Product)
            .Where(p => p.IsActive && p.StartDate <= today && p.EndDate >= today)
            .OrderBy(p => p.EndDate)
            .ToList();

        return View(promotions);
    }
}
