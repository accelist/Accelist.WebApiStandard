using Accelist.WebApiStandard.Logics;
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
        public async Task<string> Post([FromBody] RegisterUserRequest model)
        {
            var result = await Mediator.Send(model);
            return result;
        }
    }
}
