using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ShopVanPhongPham.Data;
using ShopVanPhongPham.Helpers;
using ShopVanPhongPham.Models;
using ShopVanPhongPham.Models.Interfaces;

namespace ShopVanPhongPham.Controllers;

public class OrdersController : Controller
{
    private readonly IOrderRepository _orderRepo;
    private readonly IShoppingCartRepository _cartRepo;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IConfiguration _config;
    private readonly AppDbContext _context;
    private (string? code, decimal discount) GetActiveDiscount(decimal subTotal)
    {
        var promoCode = HttpContext.Session.GetString("PromoCode");
        if (string.IsNullOrEmpty(promoCode)) return (null, 0);

        var today = DateTime.Today;
        var promo = _context.Promotions.FirstOrDefault(p => p.Code == promoCode);

        if (promo == null || !promo.IsActive || promo.StartDate > today || promo.EndDate < today)
        {
            HttpContext.Session.Remove("PromoCode");
            return (null, 0);
        }

        return (promo.Code, Math.Round(subTotal * promo.DiscountPercent / 100m));
    }

    private static decimal GetShippingFee(string? shippingMethod) => shippingMethod switch
    {
        "express" => 40000m,
        "pickup" => 0m,
        _ => 20000m // standard (mặc định)
    };
    public OrdersController(IOrderRepository orderRepo,
                            IShoppingCartRepository cartRepo,
                            UserManager<IdentityUser> userManager,
                            IConfiguration config,
                            AppDbContext context)
    {
        _orderRepo = orderRepo;
        _cartRepo = cartRepo;
        _context = context;
        _userManager = userManager;
        _config = config;
    }

    // GET /Orders/Checkout
    [Authorize]
    public IActionResult Checkout()
    {
        var cartItems = _cartRepo.GetCartItems();
        if (cartItems == null || !cartItems.Any())
            return RedirectToAction("Index", "ShoppingCart");

        var subTotal = _cartRepo.GetCartTotal();
        var (code, discount) = GetActiveDiscount(subTotal);

        ViewBag.SubTotal = subTotal;
        ViewBag.PromoCode = code;
        ViewBag.Discount = discount;
        ViewBag.DiscountPercent = code != null
            ? _context.Promotions.FirstOrDefault(p => p.Code == code)?.DiscountPercent
            : null;

        var bank = _config.GetSection("BankInfo");
        ViewBag.QrPreviewUrl = VietQrHelper.BuildQrUrl(
            bank["BankId"]!, bank["AccountNo"]!, bank["AccountName"]!,
            subTotal - discount, "Thanh toan VPP Shop");

        return View(cartItems);
    }
    // POST /Orders/Checkout
    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Checkout(
     string firstName, string lastName,
     string phone, string address,
     string paymentMethod, string? shippingMethod)
    {
        if (string.IsNullOrWhiteSpace(firstName) ||
            string.IsNullOrWhiteSpace(lastName) ||
            string.IsNullOrWhiteSpace(phone) ||
            string.IsNullOrWhiteSpace(address))
        {
            ModelState.AddModelError("", "Vui lòng điền đầy đủ thông tin.");
            return View(_cartRepo.GetCartItems());
        }

        var user = await _userManager.GetUserAsync(User);
        var userEmail = user?.Email ?? "";

        var cartItems = _cartRepo.GetCartItems();
        var subTotal = _cartRepo.GetCartTotal();
        var (promoCode, discount) = GetActiveDiscount(subTotal);
        var shippingFee = GetShippingFee(shippingMethod);

        var order = new Order
        {
            FirstName = firstName,
            LastName = lastName,
            Email = userEmail,
            Phone = phone,
            Address = address,
            OrderTotal = subTotal - discount + shippingFee,
            PromotionCode = promoCode,
            DiscountAmount = discount,
            OrderPlaced = DateTime.Now,
            PaymentMethod = string.IsNullOrWhiteSpace(paymentMethod) ? "COD" : paymentMethod,
            PaymentStatus = "Chưa thanh toán",
            OrderDetails = cartItems.Select(item => new OrderDetail
            {
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                Price = item.Product!.Price
            }).ToList()
        };

        var placedOrder = _orderRepo.PlaceOrder(order);
        _cartRepo.ClearCart();
        HttpContext.Session.SetInt32("CartCount", 0);
        HttpContext.Session.Remove("PromoCode");

        return RedirectToAction("CheckoutComplete", new { orderId = placedOrder.Id });
    }

    // GET /Orders/CheckoutComplete
    public IActionResult CheckoutComplete(int orderId)
    {
        var order = _orderRepo.GetOrderById(orderId);
        if (order == null) return RedirectToAction("Index", "Home");

        ViewBag.OrderId = orderId;

        if (order.PaymentMethod == "QR")
        {
            var bank = _config.GetSection("BankInfo");
            ViewBag.QrUrl = VietQrHelper.BuildQrUrl(
                bank["BankId"]!, bank["AccountNo"]!, bank["AccountName"]!,
                order.OrderTotal, $"DH{order.Id}");
        }

        return View(order);
    }

    // GET /Orders/MyOrders
    [Authorize]
    public async Task<IActionResult> MyOrders()
    {
        var user = await _userManager.GetUserAsync(User);
        var orders = _orderRepo.GetAllOrders()
                               .Where(o => o.Email == user!.Email)
                               .OrderByDescending(o => o.OrderPlaced)
                               .ToList();
        return View(orders);
    }
}