#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace ShopVanPhongPham.Areas.Identity.Pages.Account
{
    public class ResetPasswordModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;

        public ResetPasswordModel(UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "Vui lòng nh?p m?t kh?u m?i")]
            [StringLength(100, ErrorMessage = "M?t kh?u ph?i có ít nh?t {2} ký t?.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            public string Password { get; set; } = "";

            [Required(ErrorMessage = "Vui lòng xác nh?n m?t kh?u")]
            [DataType(DataType.Password)]
            [Compare("Password", ErrorMessage = "M?t kh?u xác nh?n không kh?p.")]
            public string ConfirmPassword { get; set; } = "";

            [Required]
            public string Code { get; set; }

            [Required]
            [EmailAddress]
            public string Email { get; set; }
        }

        public IActionResult OnGet(string code = null, string email = null)
        {
            if (code == null || email == null)
            {
                return BadRequest("Liên k?t ??t l?i m?t kh?u không h?p l?.");
            }

            Input = new InputModel
            {
                Code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code)),
                Email = email
            };
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await _userManager.FindByEmailAsync(Input.Email);
            if (user == null)
            {
                // Không ti?t l? vi?c email có t?n t?i hay không
                return RedirectToPage("./ResetPasswordConfirmation");
            }

            var result = await _userManager.ResetPasswordAsync(user, Input.Code, Input.Password);
            if (result.Succeeded)
            {
                return RedirectToPage("./ResetPasswordConfirmation");
            }

            foreach (var error in result.Errors)
            {
                var message = error.Code switch
                {
                    "PasswordTooShort" => "M?t kh?u ph?i có ít nh?t 6 ký t?.",
                    "PasswordRequiresNonAlphanumeric" => "M?t kh?u ph?i có ít nh?t 1 ký t? ??c bi?t (vd: @, #, !).",
                    "PasswordRequiresDigit" => "M?t kh?u ph?i có ít nh?t 1 ch? s? (0-9).",
                    "PasswordRequiresLower" => "M?t kh?u ph?i có ít nh?t 1 ch? th??ng (a-z).",
                    "PasswordRequiresUpper" => "M?t kh?u ph?i có ít nh?t 1 ch? hoa (A-Z).",
                    "InvalidToken" => "Liên k?t ??t l?i m?t kh?u ?ã h?t h?n ho?c không h?p l?.",
                    _ => error.Description
                };
                ModelState.AddModelError(string.Empty, message);
            }
            return Page();
        }
    }
}
