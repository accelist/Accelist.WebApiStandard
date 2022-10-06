using MediatR;

namespace Accelist.WebApiStandard.Contracts.RequestModels
{
    public class DemoKafkaRequest : IRequest
    {
        public static string Topic => typeof(DemoKafkaRequest).FullName ?? nameof(DemoKafkaRequest);

        public int A { set; get; }

        public int B { set; get; }
    }
}
