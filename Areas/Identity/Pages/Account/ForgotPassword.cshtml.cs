#nullable disable

using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShopVanPhongPham.Services;

namespace ShopVanPhongPham.Areas.Identity.Pages.Account
{
    public class ForgotPasswordModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IVerificationCodeService _codeService;

        public ForgotPasswordModel(UserManager<IdentityUser> userManager, IVerificationCodeService codeService)
        {
            _userManager = userManager;
            _codeService = codeService;
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

            var user = await _userManager.FindByEmailAsync(Input.Email);
            if (user != null)
            {
                var result = await _codeService.SendAsync(Input.Email, CodePurpose.ResetPassword);

                if (result.Status == SendCodeStatus.Failed)
                {
                    ModelState.AddModelError(string.Empty, "Không gửi được email lúc này. Vui lòng thử lại sau.");
                    return Page();
                }

                if (result.Status == SendCodeStatus.TooSoon)
                {
                    
                    TempData["StatusMessage"] =
                        $"Mã xác nhận vừa được gửi trước đó, vui lòng kiểm tra email. Bạn có thể gửi lại sau {result.WaitSeconds} giây.";
                    return RedirectToPage("./ResetPassword", new { email = Input.Email });
                }
            }

            
            TempData["StatusMessage"] =
                "Nếu email tồn tại trong hệ thống, mã xác nhận gồm 6 số đã được gửi tới hộp thư của bạn (có hiệu lực 10 phút).";
            return RedirectToPage("./ResetPassword", new { email = Input.Email });
        }
    }
}