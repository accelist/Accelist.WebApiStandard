using MediatR;

namespace Accelist.WebApiStandard.Contracts.RequestModels
{
    public class DemoKafkaRequest : IRequest
    {
        public int A { set; get; }

        public int B { set; get; }
    }
}
