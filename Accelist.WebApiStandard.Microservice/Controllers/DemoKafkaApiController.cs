using Accelist.WebApiStandard.Contracts.RequestModels;
using Accelist.WebApiStandard.Services.Kafka;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Accelist.WebApiStandard.Microservice.Controllers
{
    [Route("api/demo-kafka")]
    [ApiController]
    public class DemoKafkaApiController : ControllerBase
    {
        private readonly KafkaJsonProducer _kafkaJsonProducer;

        public DemoKafkaApiController(KafkaJsonProducer kafkaJsonProducer)
        {
            _kafkaJsonProducer = kafkaJsonProducer;
        }

        [HttpPost]
        public async Task<ActionResult<long>> Post([FromBody] DemoKafkaRequest model, CancellationToken cancellationToken)
        {
            var result = await _kafkaJsonProducer.ProduceAsync(model, cancellationToken);

            if (result == null)
            {
                return Problem("Failed to send message to Kafka");
            }

            return result;
        }
    }
}
