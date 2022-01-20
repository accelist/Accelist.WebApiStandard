using Accelist.WebApiStandard.Logics.Requests;
using MediatR;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Accelist.WebApiStandard.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IMediator Mediator;

        public UserController(IMediator mediator)
        {
            this.Mediator = mediator;
        }

        // POST api/<UserController>
        [HttpPost]
        public async Task<ActionResult<string>> Post([FromBody] RegisterUserRequest model, CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(model, cancellationToken);
            // if needed, the result can be re-sent into Mediator as another request
            // useful when creating logic pipeline for a complex app!
            // model error can be added manually using method           : ModelState.AddModelError("...", "...");
            // RFC 7807 compliant response can be produced using method : return ValidationProblem(ModelState);
            return result;
        }
    }
}
