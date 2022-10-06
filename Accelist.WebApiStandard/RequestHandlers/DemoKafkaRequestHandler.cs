using Accelist.WebApiStandard.Contracts.RequestModels;
using MediatR;

namespace Accelist.WebApiStandard.RequestHandlers
{
    public class DemoKafkaRequestHandler : IRequestHandler<DemoKafkaRequest>
    {
        public Task<Unit> Handle(DemoKafkaRequest request, CancellationToken cancellationToken)
        {
            Console.WriteLine(request.A + request.B);

            return Unit.Task;
        }
    }
}
