using Microsoft.AspNetCore.Mvc;

namespace Accelist.WebApiStandard.Microservice.Controllers
{
    [ApiController]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class ErrorController : ControllerBase
    {
        /// <summary>
        /// https://learn.microsoft.com/en-us/aspnet/core/web-api/handle-errors?view=aspnetcore-6.0#exception-handler-1
        /// </summary>
        /// <returns></returns>
        [Route("/error")]
        public IActionResult HandleError() => Problem();
    }
}
