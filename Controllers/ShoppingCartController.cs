using Microsoft.AspNetCore.Mvc;
using ShopVanPhongPham.Data;
using ShopVanPhongPham.Models.Interfaces;

namespace ShopVanPhongPham.Controllers
{
    public class ShoppingCartController : Controller
    {
        private readonly IShoppingCartRepository _cart;
        private readonly AppDbContext _context;

        public ShoppingCartController(IShoppingCartRepository cart, AppDbContext context)
        {
            _cart = cart;
            _context = context;
        }


        public IActionResult Index()
        {
            var items = _cart.GetCartItems();
            var subTotal = _cart.GetCartTotal();

            decimal discount = 0;
            var promoCode = HttpContext.Session.GetString("PromoCode");

            if (!string.IsNullOrEmpty(promoCode))
            {
                var promo = _context.Promotions.FirstOrDefault(p => p.Code == promoCode);
                var today = DateTime.Today;

                if (promo == null || !promo.IsActive || promo.StartDate > today || promo.EndDate < today)
                {
                    HttpContext.Session.Remove("PromoCode");
                    TempData["ErrorMessage"] = "Mã giảm giá đã hết hạn hoặc không còn hiệu lực.";
                }
                else
                {
                    discount = Math.Round(subTotal * promo.DiscountPercent / 100m);
                    ViewBag.PromoCode = promo.Code;
                    ViewBag.DiscountPercent = promo.DiscountPercent;
                }
            }

            ViewBag.CartCount = _cart.GetCartCount();
            ViewBag.SubTotal = subTotal;
            ViewBag.Discount = discount;
            ViewBag.CartTotal = subTotal - discount;

            return View(items);
        }


        [HttpPost]
        public IActionResult ApplyPromotion(string code, string? returnUrl)
        {
            if (!User.Identity!.IsAuthenticated)
                return RedirectToLogin(returnUrl ?? Url.Action("Index"));

            if (string.IsNullOrWhiteSpace(code))
            {
                TempData["ErrorMessage"] = "Vui lòng nhập mã giảm giá.";
                return Redirect(returnUrl ?? Url.Action("Index")!);
            }

            var today = DateTime.Today;
            var promo = _context.Promotions.FirstOrDefault(p => p.Code == code.Trim());

            if (promo == null)
            {
                TempData["ErrorMessage"] = "Mã giảm giá không tồn tại.";
            }
            else if (!promo.IsActive || promo.StartDate > today || promo.EndDate < today)
            {
                TempData["ErrorMessage"] = "Mã giảm giá đã hết hạn hoặc chưa bắt đầu.";
            }
            else
            {
                HttpContext.Session.SetString("PromoCode", promo.Code);
                TempData["SuccessMessage"] = $"Đã áp dụng mã \"{promo.Code}\" — giảm {promo.DiscountPercent}%!";
            }

            return Redirect(returnUrl ?? Url.Action("Index")!);
        }


        [HttpPost]
        public IActionResult RemovePromotion(string? returnUrl)
        {
            HttpContext.Session.Remove("PromoCode");
            TempData["SuccessMessage"] = "Đã hủy mã giảm giá.";
            return Redirect(returnUrl ?? Url.Action("Index")!);
        }

        [HttpPost]
        public IActionResult AddToCart(int productId, int quantity = 1)
        {
            bool wantsJson = Request.Headers["Accept"].ToString().Contains("application/json");

            if (!User.Identity!.IsAuthenticated)
            {
                if (wantsJson)
                    return Json(new { success = false, requiresLogin = true });
                return RedirectToLogin(Url.Action("Detail", "Product", new { id = productId }));
            }

            var product = _context.Products.Find(productId);
            if (product == null)
            {
                if (wantsJson)
                    return Json(new { success = false, message = "Sản phẩm không tồn tại." });
                return NotFound();
            }


            if (product.Stock <= 0)
            {
                var msg = $"\"{product.Name}\" đã hết hàng.";
                if (wantsJson)
                    return Json(new { success = false, message = msg });
                TempData["ErrorMessage"] = msg;
                return RedirectToAction("Detail", "Product", new { id = productId });
            }


            var currentQty = _cart.GetCartItems()
                .FirstOrDefault(x => x.ProductId == productId)?.Quantity ?? 0;

            if (currentQty + quantity > product.Stock)
            {
                var msg = $"Chỉ còn {product.Stock} \"{product.Name}\" trong kho.";
                if (wantsJson)
                    return Json(new { success = false, message = msg });
                TempData["ErrorMessage"] = msg;
                return RedirectToAction("Detail", "Product", new { id = productId });
            }

            _cart.AddToCart(product, quantity);

            if (wantsJson)
                return Json(new { success = true, cartCount = _cart.GetCartCount(), productName = product.Name });

            TempData["SuccessMessage"] = $"Đã thêm \"{product.Name}\" vào giỏ hàng!";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Remove(int id)
        {
            if (!User.Identity!.IsAuthenticated)
                return RedirectToLogin(Url.Action("Index"));

            _cart.RemoveFromCart(id);
            TempData["SuccessMessage"] = "Đã xóa sản phẩm khỏi giỏ hàng.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Increase(int productId)
        {
            if (!User.Identity!.IsAuthenticated)
                return RedirectToLogin(Url.Action("Index"));


            var product = _context.Products.Find(productId);
            var currentQty = _cart.GetCartItems()
                .FirstOrDefault(x => x.ProductId == productId)?.Quantity ?? 0;

            if (product != null && currentQty >= product.Stock)
            {
                TempData["ErrorMessage"] = $"Chỉ còn {product.Stock} \"{product.Name}\" trong kho.";
                return RedirectToAction("Index");
            }

            _cart.IncreaseQuantity(productId);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Decrease(int productId)
        {
            if (!User.Identity!.IsAuthenticated)
                return RedirectToLogin(Url.Action("Index"));

            _cart.DecreaseQuantity(productId);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Clear()
        {
            if (!User.Identity!.IsAuthenticated)
                return RedirectToLogin(Url.Action("Index"));

            _cart.ClearCart();
            HttpContext.Session.Remove("PromoCode");
            TempData["SuccessMessage"] = "Đã xóa toàn bộ giỏ hàng.";
            return RedirectToAction("Index");
        }

        private IActionResult RedirectToLogin(string? returnUrl)
        {
            TempData["ErrorMessage"] = "Bạn cần đăng nhập để thực hiện chức năng này.";
            return RedirectToPage("/Account/Login", new { area = "Identity", returnUrl });
        }
    }
}
