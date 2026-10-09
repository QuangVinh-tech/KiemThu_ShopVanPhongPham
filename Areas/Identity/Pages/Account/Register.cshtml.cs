#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using ShopVanPhongPham.Models.Interfaces;

namespace ShopVanPhongPham.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IUserStore<IdentityUser> _userStore;
        private readonly IUserEmailStore<IdentityUser> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IOtpService _otpService;

        public RegisterModel(
            UserManager<IdentityUser> userManager,
            IUserStore<IdentityUser> userStore,
            SignInManager<IdentityUser> signInManager,
            ILogger<RegisterModel> logger,
            IOtpService otpService)
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _otpService = otpService;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "Vui lòng nhập họ và tên")]
            [StringLength(100, ErrorMessage = "Họ và tên tối đa {1} ký tự.")]
            public string FullName { get; set; } = "";

            [Required(ErrorMessage = "Vui lòng nhập email")]
            [EmailAddress(ErrorMessage = "Email không hợp lệ")]
            public string Email { get; set; } = "";

            [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
            [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
            public string PhoneNumber { get; set; } = "";

            [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
            [StringLength(100, ErrorMessage = "Mật khẩu phải có ít nhất {2} ký tự.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            public string Password { get; set; } = "";

            [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu")]
            [Compare("Password", ErrorMessage = "Mật khẩu xác nhận không khớp.")]
            [DataType(DataType.Password)]
            public string ConfirmPassword { get; set; } = "";
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (!ModelState.IsValid)
                return Page();

            var email = Input.Email.Trim();

            // Email đã tồn tại?
            var existing = await _userManager.FindByEmailAsync(email);
            if (existing != null)
            {
                if (existing.EmailConfirmed)
                {
                    ModelState.AddModelError(string.Empty, "Email này đã được đăng ký.");
                    return Page();
                }

                // Đã đăng ký nhưng chưa xác thực: gửi lại OTP
                var (resent, resentMsg) = await _otpService.SendOtpAsync(email, OtpPurposes.Register);
                if (resent)
                    TempData["OtpInfo"] = "Email này đã đăng ký nhưng chưa xác thực. Chúng tôi đã gửi lại mã OTP.";
                else
                    TempData["OtpError"] = resentMsg;

                return RedirectToPage("./VerifyOtp", new { email, returnUrl });
            }

            var user = CreateUser();
            await _userStore.SetUserNameAsync(user, email, CancellationToken.None);
            await _emailStore.SetEmailAsync(user, email, CancellationToken.None);
            var result = await _userManager.CreateAsync(user, Input.Password);

            if (result.Succeeded)
            {
                _logger.LogInformation("User created a new account with password.");

                await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
                await _userManager.AddClaimAsync(user, new System.Security.Claims.Claim("FullName", Input.FullName));

                // Gửi OTP xác thực email, CHƯA đăng nhập cho tới khi nhập đúng mã
                var (sent, message) = await _otpService.SendOtpAsync(email, OtpPurposes.Register);
                if (sent)
                    TempData["OtpInfo"] = message;
                else
                    TempData["OtpError"] = message;

                return RedirectToPage("./VerifyOtp", new { email, returnUrl });
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
                    "DuplicateEmail" => "Email này đã được đăng ký.",
                    "DuplicateUserName" => "Tên tài khoản đã tồn tại.",
                    "InvalidEmail" => "Email không hợp lệ.",
                    _ => error.Description
                };
                ModelState.AddModelError(string.Empty, message);
            }

            return Page();
        }

        private IdentityUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<IdentityUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(IdentityUser)}'. " +
                    $"Ensure that '{nameof(IdentityUser)}' is not an abstract class and has a parameterless constructor.");
            }
        }

        private IUserEmailStore<IdentityUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<IdentityUser>)_userStore;
        }
    }
}