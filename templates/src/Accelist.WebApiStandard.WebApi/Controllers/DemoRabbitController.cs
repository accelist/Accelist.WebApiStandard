using Accelist.WebApiStandard.Contracts.RabbitMqMessages;
using MassTransit;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Accelist.WebApiStandard.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DemoRabbitController : ControllerBase
    {
        // When publishing from Mediator, use IBus instead of IPublishEndpoint
        // IPublishEndpoint only works from the Controller level
        private readonly IPublishEndpoint _publish;

        public DemoRabbitController(IPublishEndpoint publish)
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
