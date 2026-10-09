#nullable disable

using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShopVanPhongPham.Services;

namespace ShopVanPhongPham.Areas.Identity.Pages.Account
{
    public class ResetPasswordModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IVerificationCodeService _codeService;

        public ResetPasswordModel(UserManager<IdentityUser> userManager, IVerificationCodeService codeService)
        {
            _userManager = userManager;
            _codeService = codeService;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; } = "";

            [Required(ErrorMessage = "Vui lòng nh?p mã xác nh?n")]
            [RegularExpression(@"^\d{6}$", ErrorMessage = "Mã xác nh?n g?m ?úng 6 ch? s?")]
            public string Code { get; set; } = "";

            [Required(ErrorMessage = "Vui lòng nh?p m?t kh?u m?i")]
            [StringLength(100, ErrorMessage = "M?t kh?u ph?i có ít nh?t {2} ký t?.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            public string Password { get; set; } = "";

            [Required(ErrorMessage = "Vui lòng xác nh?n m?t kh?u")]
            [Compare("Password", ErrorMessage = "M?t kh?u xác nh?n không kh?p.")]
            [DataType(DataType.Password)]
            public string ConfirmPassword { get; set; } = "";
        }

        public IActionResult OnGet(string email = null)
        {
            if (string.IsNullOrWhiteSpace(email))
                return RedirectToPage("./ForgotPassword");

            Input = new InputModel { Email = email };
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var user = await _userManager.FindByEmailAsync(Input.Email);
            var status = _codeService.Verify(Input.Email, CodePurpose.ResetPassword, Input.Code);

            if (user == null || status != VerifyCodeStatus.Success)
            {
                ModelState.AddModelError(string.Empty, status == VerifyCodeStatus.TooManyAttempts
                    ? "B?n ?ã nh?p sai quá nhi?u l?n. Vui lòng b?m \"G?i l?i mã\" ?? nh?n mã m?i."
                    : "Mã xác nh?n không ?úng ho?c ?ã h?t h?n.");
                return Page();
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, Input.Password);

            if (result.Succeeded)
            {
                // Ng??i dùng ?ã ch?ng minh s? h?u email -> coi nh? ?ã xác minh email.
                if (!user.EmailConfirmed)
                {
                    user.EmailConfirmed = true;
                    await _userManager.UpdateAsync(user);
                }

                _codeService.Remove(Input.Email, CodePurpose.ResetPassword);
                return RedirectToPage("./ResetPasswordConfirmation");
            }

            // L?i chính sách m?t kh?u: gi? nguyên mã (còn hi?u l?c) ?? ng??i dùng nh?p l?i m?t kh?u.
            foreach (var error in result.Errors)
            {
                var message = error.Code switch
                {
                    "PasswordTooShort" => "M?t kh?u ph?i có ít nh?t 6 ký t?.",
                    "PasswordRequiresNonAlphanumeric" => "M?t kh?u ph?i có ít nh?t 1 ký t? ??c bi?t (vd: @, #, !).",
                    "PasswordRequiresDigit" => "M?t kh?u ph?i có ít nh?t 1 ch? s? (0-9).",
                    "PasswordRequiresLower" => "M?t kh?u ph?i có ít nh?t 1 ch? th??ng (a-z).",
                    "PasswordRequiresUpper" => "M?t kh?u ph?i có ít nh?t 1 ch? hoa (A-Z).",
                    _ => error.Description
                };
                ModelState.AddModelError(string.Empty, message);
            }
            return Page();
        }

        public async Task<IActionResult> OnPostResendAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return RedirectToPage("./ForgotPassword");

            var user = await _userManager.FindByEmailAsync(email);
            if (user != null)
            {
                var result = await _codeService.SendAsync(email, CodePurpose.ResetPassword);
                if (result.Status == SendCodeStatus.TooSoon)
                {
                    ErrorMessage = $"Vui lòng ??i {result.WaitSeconds} giây r?i g?i l?i mã.";
                    return RedirectToPage("./ResetPassword", new { email });
                }
                if (result.Status == SendCodeStatus.Failed)
                {
                    ErrorMessage = "Không g?i ???c email lúc này. Vui lòng th? l?i sau.";
                    return RedirectToPage("./ResetPassword", new { email });
                }
            }

            StatusMessage = "N?u email t?n t?i trong h? th?ng, mã m?i ?ã ???c g?i t?i h?p th? c?a b?n.";
            return RedirectToPage("./ResetPassword", new { email });
        }
    }
}