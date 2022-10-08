using Accelist.WebApiStandard.Contracts.RequestModels;
using MassTransit;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Accelist.WebApiStandard.WebApi.V1
{
    [Route("api/[controller]")]
    [ApiController]
    public class DemoRabbitController : ControllerBase
    {
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
