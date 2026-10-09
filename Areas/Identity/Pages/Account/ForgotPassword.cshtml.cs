#nullable disable

using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShopVanPhongPham.Models.Interfaces;

namespace ShopVanPhongPham.Areas.Identity.Pages.Account
{
    public class ForgotPasswordModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IOtpService _otpService;

        public ForgotPasswordModel(UserManager<IdentityUser> userManager, IOtpService otpService)
        {
            _userManager = userManager;
            _otpService = otpService;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "Vui lòng nhập email")]
            [EmailAddress(ErrorMessage = "Email không hợp lệ")]
            public string Email { get; set; } = "";
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var email = Input.Email.Trim();
            var user = await _userManager.FindByEmailAsync(email);

            // Luôn hiện cùng một thông báo để không lộ email nào đã đăng ký
            TempData["OtpInfo"] = "Nếu email tồn tại trong hệ thống, mã OTP đã được gửi. Vui lòng kiểm tra hộp thư (kể cả mục spam).";

            if (user != null)
            {
                var (sent, message) = await _otpService.SendOtpAsync(email, OtpPurposes.ResetPassword);
                if (!sent)
                {
                    TempData.Remove("OtpInfo");
                    TempData["OtpError"] = message;
                }
            }

            return RedirectToPage("./ResetPassword", new { email });
        }
    }
}