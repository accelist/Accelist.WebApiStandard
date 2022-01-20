using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Accelist.WebApiStandard.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomErrorController : ControllerBase
    {
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            // if you need to return custom error code for customer workflow purposes:

            return Problem(
                statusCode: 400,                                    // RFC 7807: must be HTTP Status Code (400)
                type: "https://www.accelist.com/problem/ACL001",    // RFC 7807: must be valid URL (error description page)
                title: "Car Yard is Full",
                detail: "The car with VIN: 1HGBH41JXMN109186 cannot be placed in the yard XYZ due to capacity limit."
            );
        }
    }
}
