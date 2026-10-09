#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShopVanPhongPham.Models.Interfaces;

namespace ShopVanPhongPham.Areas.Identity.Pages.Account
{
    public class ResetPasswordModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IOtpService _otpService;

        public ResetPasswordModel(UserManager<IdentityUser> userManager, IOtpService otpService)
        {
            _userManager = userManager;
            _otpService = otpService;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        [TempData]
        public string OtpInfo { get; set; }

        [TempData]
        public string OtpError { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "Vui lòng nhập email")]
            [EmailAddress(ErrorMessage = "Email không hợp lệ")]
            public string Email { get; set; }

            [Required(ErrorMessage = "Vui lòng nhập mã OTP")]
            [RegularExpression(@"^\d{6}$", ErrorMessage = "Mã OTP gồm đúng 6 chữ số")]
            public string Code { get; set; }

            [Required(ErrorMessage = "Vui lòng nhập mật khẩu mới")]
            [StringLength(100, ErrorMessage = "Mật khẩu phải có ít nhất {2} ký tự.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            public string Password { get; set; } = "";

            [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu")]
            [DataType(DataType.Password)]
            [Compare("Password", ErrorMessage = "Mật khẩu xác nhận không khớp.")]
            public string ConfirmPassword { get; set; } = "";
        }

        public IActionResult OnGet(string email = null)
        {
            if (string.IsNullOrWhiteSpace(email))
                return RedirectToPage("./ForgotPassword");

            Input = new InputModel { Email = email };
            return Page();
        }

        // Xác nhận OTP + đặt mật khẩu mới
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var email = Input.Email.Trim();
            var user = await _userManager.FindByEmailAsync(email);

            // Chỉ kiểm tra, chưa hủy mã: nếu mật khẩu mới chưa đạt yêu cầu thì vẫn dùng lại được mã này
            var check = _otpService.Verify(email, OtpPurposes.ResetPassword, Input.Code, consumeOnSuccess: false);
            if (user == null || check != OtpVerifyResult.Success)
            {
                var errorResult = check == OtpVerifyResult.Success ? OtpVerifyResult.Expired : check;
                ModelState.AddModelError(string.Empty, OtpMessages.For(errorResult));
                return Page();
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, Input.Password);

            if (result.Succeeded)
            {
                _otpService.Invalidate(email, OtpPurposes.ResetPassword);

                // Đã chứng minh sở hữu email bằng OTP nên coi như email đã xác thực
                if (!user.EmailConfirmed)
                {
                    user.EmailConfirmed = true;
                    await _userManager.UpdateAsync(user);
                }

                return RedirectToPage("./ResetPasswordConfirmation");
            }

            foreach (var error in result.Errors)
            {
                var message = error.Code switch
                {
                    "PasswordTooShort" => "Mật khẩu phải có ít nhất 6 ký tự.",
                    "PasswordRequiresNonAlphanumeric" => "Mật khẩu phải có ít nhất 1 ký tự đặc biệt (vd: @, #, !).",
                    "PasswordRequiresDigit" => "Mật khẩu phải có ít nhất 1 chữ số (0-9).",
                    "PasswordRequiresLower" => "Mật khẩu phải có ít nhất 1 chữ thường (a-z).",
                    "PasswordRequiresUpper" => "Mật khẩu phải có ít nhất 1 chữ hoa (A-Z).",
                    _ => error.Description
                };
                ModelState.AddModelError(string.Empty, message);
            }

            return Page();
        }

        // Gửi lại mã OTP
        public async Task<IActionResult> OnPostResendAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return RedirectToPage("./ForgotPassword");

            email = email.Trim();
            var user = await _userManager.FindByEmailAsync(email);

            OtpInfo = "Nếu email tồn tại trong hệ thống, mã OTP đã được gửi. Vui lòng kiểm tra hộp thư (kể cả mục spam).";

            if (user != null)
            {
                var (sent, message) = await _otpService.SendOtpAsync(email, OtpPurposes.ResetPassword);
                if (!sent)
                {
                    OtpInfo = null;
                    OtpError = message;
                }
            }

            return RedirectToPage(new { email });
        }
    }
}