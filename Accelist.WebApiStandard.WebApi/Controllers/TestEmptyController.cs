using Accelist.WebApiStandard.Contracts.RequestModels;
using MediatR;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Accelist.WebApiStandard.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestEmptyController : ControllerBase
    {
        private readonly IMediator _mediator;

        public TestEmptyController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<ActionResult<bool>> Get(CancellationToken cancellationToken)
        {
            var req = new TestEmptyRequest();
            var res = await _mediator.Send(req, cancellationToken);
            return res.Success;
        }
    }
}
