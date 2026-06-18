using Microsoft.Extensions.DependencyInjection;

namespace KnowledgeAi.Application.Common.Mediator;

internal abstract class RequestHandlerWrapperBase
{
    public abstract Task<object?> Handle(object request, IServiceProvider provider, CancellationToken cancellationToken);
}

internal sealed class RequestHandlerWrapper<TRequest, TResponse> : RequestHandlerWrapperBase
    where TRequest : IRequest<TResponse>
{
    public override async Task<object?> Handle(object request, IServiceProvider provider, CancellationToken cancellationToken)
    {
        var typedRequest = (TRequest)request;

        RequestHandlerDelegate<TResponse> pipeline = () =>
        {
            var handler = provider.GetRequiredService<IRequestHandler<TRequest, TResponse>>();
            return handler.Handle(typedRequest, cancellationToken);
        };

        var behaviors = provider.GetServices<IPipelineBehavior<TRequest, TResponse>>().Reverse();

        foreach (var behavior in behaviors)
        {
            var next = pipeline;
            pipeline = () => behavior.Handle(typedRequest, next, cancellationToken);
        }

        return await pipeline();
    }
}
