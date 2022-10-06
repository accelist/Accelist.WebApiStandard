using Accelist.WebApiStandard.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Accelist.WebApiStandard.Microservice.Pages
{
    public class IndexModel : PageModel
    {
        private readonly AppSettings _settings;

        public IndexModel(AppSettings appSettings)
        {
            _settings = appSettings;
        }

        public IActionResult OnGet()
        {
            if (User.Identity?.IsAuthenticated != true)
            {
                return Challenge();
            }

            return Redirect(_settings.FrontEndHost);
        }
    }
}