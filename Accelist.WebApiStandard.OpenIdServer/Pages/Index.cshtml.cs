using Accelist.WebApiStandard.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;

namespace Accelist.WebApiStandard.OpenIdServer.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IOptions<AppSettings> _options;

        public IndexModel(IOptions<AppSettings> options)
        {
            _options = options;
        }

        public IActionResult OnGet()
        {
            if (User.Identity?.IsAuthenticated != true)
            {
                return Challenge();
            }

            return Redirect(_options.Value.FrontEndHost);
        }
    }
}