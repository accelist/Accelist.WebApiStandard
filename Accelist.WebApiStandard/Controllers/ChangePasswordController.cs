using Accelist.WebApiStandard.Logics.Requests;
using MediatR;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Accelist.WebApiStandard.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChangePasswordController : ControllerBase
    {
        private readonly IMediator Mediator;

        public ChangePasswordController(IMediator mediator)
        {
            this.Mediator = mediator;
        }

        // POST api/<ChangePasswordController>
        [HttpPost]
        public async Task<bool> Post([FromBody] ChangePasswordRequestModel model, CancellationToken cancellationToken)
        {
            var request = new ChangePasswordRequest
            {
                UserID = User.FindFirst("UserId")?.Value,
                Model = model
            };
            var response = await Mediator.Send(request, cancellationToken);
            return response;
        }
    }
}
