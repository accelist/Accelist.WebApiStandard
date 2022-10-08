using Accelist.WebApiStandard.WebApi.AuthorizationPolicies;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Accelist.WebApiStandard.WebApi.V2
{
    /// <summary>
    /// Represents a REST service for testing API Versioning.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    // https://github.com/dotnet/aspnet-api-versioning/wiki/Versioning-via-the-Query-String
    [ApiVersion("2.0")]
    [Authorize(AuthorizationPolicyNames.ScopeApi)]
    public class TestController : ControllerBase
    {
        /// <summary>
        /// Gets an array of string appended with string value from query string
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IEnumerable<string> Get(string value = "")
        {
            return new string[] { "value1", "value2", value };
        }
    }
}
