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
using ShopVanPhongPham.Services;

namespace ShopVanPhongPham.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IUserStore<IdentityUser> _userStore;
        private readonly IUserEmailStore<IdentityUser> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IVerificationCodeService _codeService;

        public RegisterModel(
            UserManager<IdentityUser> userManager,
            IUserStore<IdentityUser> userStore,
            SignInManager<IdentityUser> signInManager,
            ILogger<RegisterModel> logger,
            IVerificationCodeService codeService)
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _codeService = codeService;
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
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid)
            {
                // Email đã tồn tại?
                var existing = await _userManager.FindByEmailAsync(Input.Email);
                if (existing != null)
                {
                    if (!existing.EmailConfirmed)
                    {
                        // Đã đăng ký nhưng chưa xác minh -> gửi lại mã, chuyển sang trang nhập mã.
                        var resend = await _codeService.SendAsync(Input.Email, CodePurpose.VerifyEmail);
                        TempData["StatusMessage"] = resend.Status == SendCodeStatus.Failed
                            ? null
                            : "Email này đã đăng ký nhưng chưa được xác minh. Vui lòng nhập mã 6 số đã gửi tới email của bạn.";
                        if (resend.Status == SendCodeStatus.Failed)
                            TempData["ErrorMessage"] = "Không gửi được email lúc này. Vui lòng bấm \"Gửi lại mã\" sau ít phút.";

                        return RedirectToPage("./VerifyEmail", new { email = Input.Email, returnUrl });
                    }

                    ModelState.AddModelError(string.Empty, "Email này đã được đăng ký.");
                    return Page();
                }

                var user = CreateUser();

                await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);
                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
                    await _userManager.AddClaimAsync(user, new System.Security.Claims.Claim("FullName", Input.FullName));

                    // Gửi mã xác minh 6 số về email, KHÔNG đăng nhập cho tới khi xác minh xong.
                    var send = await _codeService.SendAsync(Input.Email, CodePurpose.VerifyEmail);
                    if (send.Status == SendCodeStatus.Failed)
                        TempData["ErrorMessage"] = "Tạo tài khoản thành công nhưng chưa gửi được email. Vui lòng bấm \"Gửi lại mã\".";
                    else
                        TempData["StatusMessage"] = "Chúng tôi đã gửi mã xác minh gồm 6 số tới email của bạn.";

                    return RedirectToPage("./VerifyEmail", new { email = Input.Email, returnUrl });
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
            }

            // If we got this far, something failed, redisplay form
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
                    $"Ensure that '{nameof(IdentityUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
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