#nullable disable

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ShopVanPhongPham.Areas.Identity.Pages.Account
{
    public class ForgotPasswordConfirmationModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public string Link { get; set; }

        public void OnGet()
        {
        }
    }
}
