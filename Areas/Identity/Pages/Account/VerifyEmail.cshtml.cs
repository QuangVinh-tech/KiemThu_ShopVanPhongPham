#nullable disable

using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShopVanPhongPham.Services;

namespace ShopVanPhongPham.Areas.Identity.Pages.Account
{
    public class VerifyEmailModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IVerificationCodeService _codeService;

        public VerifyEmailModel(UserManager<IdentityUser> userManager, IVerificationCodeService codeService)
        {
            _userManager = userManager;
            _codeService = codeService;
        }

        [BindProperty(SupportsGet = true)]
        public string Email { get; set; }

        [BindProperty(SupportsGet = true)]
        public string ReturnUrl { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Vui lòng nhập mã xác minh")]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "Mã xác minh gồm đúng 6 chữ số")]
        public string Code { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public IActionResult OnGet()
        {
            if (string.IsNullOrWhiteSpace(Email))
                return RedirectToPage("./Register");

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrWhiteSpace(Email))
                return RedirectToPage("./Register");

            if (!ModelState.IsValid)
                return Page();

            var user = await _userManager.FindByEmailAsync(Email);
            var status = _codeService.Verify(Email, CodePurpose.VerifyEmail, Code);

            if (user == null || status != VerifyCodeStatus.Success)
            {
                ModelState.AddModelError(string.Empty, status == VerifyCodeStatus.TooManyAttempts
                    ? "Bạn đã nhập sai quá nhiều lần. Vui lòng bấm \"Gửi lại mã\" để nhận mã mới."
                    : "Mã xác minh không đúng hoặc đã hết hạn.");
                return Page();
            }

            if (!user.EmailConfirmed)
            {
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var result = await _userManager.ConfirmEmailAsync(user, token);
                if (!result.Succeeded)
                {
                    ModelState.AddModelError(string.Empty, "Không thể xác minh email. Vui lòng thử lại.");
                    return Page();
                }
            }

            _codeService.Remove(Email, CodePurpose.VerifyEmail);

            TempData["StatusMessage"] = "Xác minh email thành công! Bạn có thể đăng nhập ngay bây giờ.";
            return RedirectToPage("./Login", new { returnUrl = ReturnUrl });
        }

        public async Task<IActionResult> OnPostResendAsync()
        {
            if (string.IsNullOrWhiteSpace(Email))
                return RedirectToPage("./Register");

            var user = await _userManager.FindByEmailAsync(Email);
            if (user != null && !user.EmailConfirmed)
            {
                var result = await _codeService.SendAsync(Email, CodePurpose.VerifyEmail);
                switch (result.Status)
                {
                    case SendCodeStatus.TooSoon:
                        ErrorMessage = $"Vui lòng đợi {result.WaitSeconds} giây rồi gửi lại mã.";
                        break;
                    case SendCodeStatus.Failed:
                        ErrorMessage = "Không gửi được email lúc này. Vui lòng thử lại sau.";
                        break;
                    default:
                        StatusMessage = "Đã gửi lại mã xác minh. Vui lòng kiểm tra email (kể cả mục spam).";
                        break;
                }
            }
            else
            {
                StatusMessage = "Nếu email cần xác minh, mã mới đã được gửi tới hộp thư của bạn.";
            }

            return RedirectToPage("./VerifyEmail", new { email = Email, returnUrl = ReturnUrl });
        }
    }
}