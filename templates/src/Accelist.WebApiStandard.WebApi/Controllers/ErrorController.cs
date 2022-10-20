using Microsoft.AspNetCore.Mvc;

namespace Accelist.WebApiStandard.WebApi.Controllers
{
    /// <summary>
    /// https://learn.microsoft.com/en-us/aspnet/core/web-api/handle-errors?view=aspnetcore-6.0#exception-handler-1
    /// </summary>
    [ApiController]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class ErrorController : ControllerBase
    {
        /// <summary>
        /// Sends an RFC 7807-compliant payload to the client.
        /// </summary>
        /// <returns></returns>
        [Route("/error")]
        public IActionResult HandleError() => Problem();
    }
}
