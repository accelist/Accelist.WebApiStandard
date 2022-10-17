using Accelist.WebApiStandard.Contracts.RequestModels;
using Accelist.WebApiStandard.Contracts.ResponseModels;
using MediatR;

namespace Accelist.WebApiStandard.RequestHandlers
{
    public class TestEmptyRequestHandler : IRequestHandler<TestEmptyRequest, TestResponse>
    {
        public Task<TestResponse> Handle(TestEmptyRequest request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new TestResponse
            {
                Success = true
            });
        }
    }
}
