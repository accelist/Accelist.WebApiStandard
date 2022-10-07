using Accelist.WebApiStandard.Contracts.RequestModels;
using MassTransit;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Accelist.WebApiStandard.Microservice.Controllers
{
    [Route("api/demo-rabbit")]
    [ApiController]
    public class DemoRabbitApiController : ControllerBase
    {
        private readonly IPublishEndpoint _publish;

        public DemoRabbitApiController(IPublishEndpoint publish)
        {
            _publish = publish;
        }

        [HttpPost]
        public async Task<ActionResult<bool>> Post([FromBody] DemoRabbitMessage model, CancellationToken cancellationToken)
        {
            await _publish.Publish(model, cancellationToken);
            return true;
        }
    }
}
