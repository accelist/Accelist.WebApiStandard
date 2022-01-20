using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Accelist.WebApiStandard.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExplodeController : ControllerBase
    {
        // GET: api/<ExplodeController>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            throw new Exception("Something happened!");
        }
    }
}
