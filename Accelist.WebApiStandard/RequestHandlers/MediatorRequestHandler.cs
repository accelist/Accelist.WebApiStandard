using MassTransit;
using MassTransit.Mediator;

namespace Accelist.WebApiStandard.RequestHandlers
{
    /// <summary>
    /// Defines an async handler for a request.
    /// </summary>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TResponse"></typeparam>
    public abstract class MediatorRequestHandler<TRequest, TResponse> : IConsumer<TRequest>
        where TRequest : class, Request<TResponse>
        where TResponse : class
    {
        public async Task Consume(ConsumeContext<TRequest> context)
        {
            var response = await Handle(context.Message, context.CancellationToken);
            await context.RespondAsync(response);
        }

        public abstract Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken);
    }

    /// <summary>
    /// Defines an async handler for a request with no (void) response.
    /// </summary>
    /// <typeparam name="TRequest"></typeparam>
    public abstract class MediatorRequestHandler<TRequest> : IConsumer<TRequest> where TRequest : class
    {
        public Task Consume(ConsumeContext<TRequest> context)
        {
            return Handle(context.Message, context.CancellationToken);
        }

        public abstract Task Handle(TRequest request, CancellationToken cancellationToken);
    }
}
