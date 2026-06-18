using System.Collections.Concurrent;

namespace KnowledgeAi.Application.Common.Mediator;

public sealed class Mediator : IMediator
{
    private static readonly ConcurrentDictionary<Type, RequestHandlerWrapperBase> WrapperCache = new();

    private readonly IServiceProvider _provider;

    public Mediator(IServiceProvider provider)
    {
        _provider = provider;
    }

    public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        var requestType = request.GetType();

        var wrapper = WrapperCache.GetOrAdd(requestType, static rt =>
        {
            var responseType = rt.GetInterfaces()
                .First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequest<>))
                .GetGenericArguments()[0];

            var wrapperType = typeof(RequestHandlerWrapper<,>).MakeGenericType(rt, responseType);
            return (RequestHandlerWrapperBase)Activator.CreateInstance(wrapperType)!;
        });

        var result = await wrapper.Handle(request, _provider, cancellationToken);
        return (TResponse)result!;
    }
}
