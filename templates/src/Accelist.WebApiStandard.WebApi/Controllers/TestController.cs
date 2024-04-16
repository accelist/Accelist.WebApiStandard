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
        /// Model for user claim types and values.
        /// </summary>
        public class DataItem
        {
            /// <summary>
            /// Gets or sets the claim type.
            /// </summary>
            public string Type { set; get; } = "";

            /// <summary>
            /// Gets or sets the claim value.
            /// </summary>
            public string Value { set; get; } = "";
        }

        /// <summary>
        /// Gets an array of user claim types and values.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult<List<DataItem>> Get()
        {
            return User.Claims.Select(Q => new DataItem
            {
                Type = Q.Type,
                Value = Q.Value
            }).ToList();
        }
    }
}
