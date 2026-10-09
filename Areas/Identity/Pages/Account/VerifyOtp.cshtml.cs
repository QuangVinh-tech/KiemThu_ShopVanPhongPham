#nullable disable

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShopVanPhongPham.Models.Interfaces;

namespace ShopVanPhongPham.Areas.Identity.Pages.Account
{
    public class VerifyOtpModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IOtpService _otpService;

        public VerifyOtpModel(UserManager<IdentityUser> userManager,
                              SignInManager<IdentityUser> signInManager,
                              IOtpService otpService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _otpService = otpService;
        }

        [BindProperty(SupportsGet = true)]
        public string Email { get; set; }

        [BindProperty(SupportsGet = true)]
        public string ReturnUrl { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Vui lòng nhập mã OTP")]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "Mã OTP gồm đúng 6 chữ số")]
        public string Code { get; set; }

        [TempData]
        public string OtpInfo { get; set; }

        [TempData]
        public string OtpError { get; set; }

        public IActionResult OnGet()
        {
            if (string.IsNullOrWhiteSpace(Email))
                return RedirectToPage("./Register");

            ReturnUrl ??= Url.Content("~/");
            return Page();
        }

        // Xác thực mã OTP
        public async Task<IActionResult> OnPostAsync()
        {
            ReturnUrl ??= Url.Content("~/");

            if (string.IsNullOrWhiteSpace(Email))
                return RedirectToPage("./Register");

            if (!ModelState.IsValid)
                return Page();

            var user = await _userManager.FindByEmailAsync(Email.Trim());
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Mã OTP đã hết hạn hoặc không tồn tại. Vui lòng gửi lại mã mới.");
                return Page();
            }

            // Tài khoản đã xác thực rồi thì không cho đăng nhập bằng OTP nữa
            if (user.EmailConfirmed)
                return RedirectToPage("./Login");

            var result = _otpService.Verify(Email.Trim(), OtpPurposes.Register, Code);
            if (result != OtpVerifyResult.Success)
            {
                ModelState.AddModelError(string.Empty, OtpMessages.For(result));
                return Page();
            }

            user.EmailConfirmed = true;
            await _userManager.UpdateAsync(user);

            await _signInManager.SignInAsync(user, isPersistent: false);
            return LocalRedirect(Url.IsLocalUrl(ReturnUrl) ? ReturnUrl : "~/");
        }

        // Gửi lại mã OTP
        public async Task<IActionResult> OnPostResendAsync()
        {
            if (string.IsNullOrWhiteSpace(Email))
                return RedirectToPage("./Register");

            var user = await _userManager.FindByEmailAsync(Email.Trim());
            if (user != null && !user.EmailConfirmed)
            {
                var (sent, message) = await _otpService.SendOtpAsync(Email.Trim(), OtpPurposes.Register);
                if (sent) OtpInfo = message;
                else OtpError = message;
            }
            else
            {
                OtpInfo = "Mã OTP đã được gửi. Vui lòng kiểm tra hộp thư (kể cả mục spam).";
            }

            return RedirectToPage(new { email = Email, returnUrl = ReturnUrl });
        }
    }
}