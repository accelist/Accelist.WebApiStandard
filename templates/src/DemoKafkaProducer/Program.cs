using Accelist.WebApiStandard.Contracts.RequestModels;
using Accelist.WebApiStandard.KafkaServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var services = new ServiceCollection();
services.AddLogging(options =>
{
    options.AddConsole();
});

// Guide: How to integrate with Apache Kafka / Redpanda in ASP.NET Core Web API
// Step 1: AddKafka to service collection
services.AddKafkaServices(options =>
{
    options.BootstrapServers = "localhost:9092";
});

var di = services.BuildServiceProvider();
using var scope = di.CreateScope();

// Step 2: Inject `KafkaJsonProducer` to the caller 
var producer = scope.ServiceProvider.GetRequiredService<KafkaJsonProducer>();

Console.Write("Enter first number: ");
var value1 = Console.ReadLine();

Console.Write("Enter second number: ");
var value2 = Console.ReadLine();

var message = new DemoKafkaRequest
{
    A = Convert.ToInt32(value1),
    B = Convert.ToInt32(value2)
};
var cts = new CancellationTokenSource(); ;

// Step 3: Send Kafka message (plain C# object)
Console.WriteLine("Sending message to Kafka topic...");
var offset = await producer.ProduceAsync(message, cts.Token);

var topicName = KafkaTopicNameTools.GetTopicNameFromType<DemoKafkaRequest>();
Console.WriteLine($"Sent message to Kafka topic `{topicName}` offset: {offset}");

// Step 4: To be continued in Accelist.WebApiStandard.KafkaWorker project, open Program.cs...
//              services.AddApplicationServices(...) for DbContext
//              services.AddMassTransitWithRabbitMq(...) for Mediator
//              services.AddKafka(...) for Kafka
// Step 5: Write MediatorRequestHandler for the Kafka message. For example, DemoKafkaRequestHandler : MediatorRequestHandler<DemoKafkaRequest>
// Step 6: Write Background Service for the Kafka message consumer. For example, DemoKafkaRequestConsumer : KafkaJsonConsumer<DemoKafkaRequest>
// Step 7: Add the background service to the hosted service collection. For example, services.AddHostedService<DemoKafkaRequestConsumer>();

// Kafka golden rule: One Consumer per Thread
// https://kafka.apache.org/23/javadoc/index.html?org/apache/kafka/clients/consumer/KafkaConsumer.html
// https://www.oreilly.com/library/view/kafka-the-definitive/9781491936153/ch04.html
// Confluent.Kafka C# client Consume method is synchronous AND blocking and each consumer group must be placed in their own thread!
