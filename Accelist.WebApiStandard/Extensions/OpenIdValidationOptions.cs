using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accelist.WebApiStandard.Extensions
{
    public class OpenIdValidationOptions
    {
        public string Authority { set; get; } = "";

        public string[] Audiences { set; get; } = Array.Empty<string>();

        public string ClientId { set; get; } = "";

        public string ClientSecret { set; get; } = "";
    }
}
