using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Accelist.WebApiStandard.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UnexpectedController : ControllerBase
    {
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            // ERROR001
            var pd = new ProblemDetails
            {
                Type = "http://www.accelist.com/error/ERROR001", // halaman mendeskripsikan errornya, definisi error code di tiap project
                Title = "Ada yang terjadi, ini harusnya jarang sih...",
                Status = 400
            };
            pd.Extensions.Add("traceId", HttpContext.TraceIdentifier);

            return BadRequest(pd);
        }
    }
}
