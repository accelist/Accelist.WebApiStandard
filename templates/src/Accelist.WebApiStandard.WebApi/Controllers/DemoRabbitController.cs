using Accelist.WebApiStandard.Contracts.RabbitMqMessages;
using MassTransit;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Accelist.WebApiStandard.WebApi.Controllers
{
    /// <summary>
    /// Demo controller for publishing a message to RabbitMQ.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class DemoRabbitController : ControllerBase
    {
        // When publishing from Mediator, use IBus instead of IPublishEndpoint
        // IPublishEndpoint only works from the Controller level
        private readonly IPublishEndpoint _publish;

        /// <summary>
        /// Constructor for DemoRabbitController.
        /// </summary>
        /// <param name="publish"></param>
        public DemoRabbitController(IPublishEndpoint publish)
        {
            _publish = publish;
        }

        /// <summary>
        /// Publishing a message to RabbitMQ example.
        /// </summary>
        /// <param name="model"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult<bool>> Post([FromBody] DemoRabbitMessage model, CancellationToken cancellationToken)
        {
            await _publish.Publish(model, cancellationToken);
            return true;
        }
    }
}
