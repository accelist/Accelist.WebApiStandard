using Accelist.WebApiStandard.WebApi.AuthorizationPolicies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Accelist.WebApiStandard.WebApi.Controllers
{
    /// <summary>
    /// Represents a REST service for testing API Versioning.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthorizationPolicyNames.ScopeApi)]
    public class TestController : ControllerBase
    {
        /// <summary>
        /// Gets an array of string
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }
    }
}
