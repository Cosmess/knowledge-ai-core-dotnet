using FluentAssertions;
using KnowledgeAi.Application.Common.Mediator;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace KnowledgeAi.UnitTests.Application.Common.Mediator;

public sealed record PingRequest(string Message) : IRequest<string>;

public sealed class PingRequestHandler : IRequestHandler<PingRequest, string>
{
    public Task<string> Handle(PingRequest request, CancellationToken cancellationToken) =>
        Task.FromResult($"pong:{request.Message}");
}

public sealed class RecordingBehavior : IPipelineBehavior<PingRequest, string>
{
    public static readonly List<string> ExecutionOrder = [];

    public async Task<string> Handle(PingRequest request, RequestHandlerDelegate<string> next, CancellationToken cancellationToken)
    {
        ExecutionOrder.Add("before");
        var response = await next();
        ExecutionOrder.Add("after");
        return response;
    }
}

public class MediatorTests
{
    private static IMediator BuildMediator()
    {
        var services = new ServiceCollection();
        services.AddScoped<IMediator, KnowledgeAi.Application.Common.Mediator.Mediator>();
        services.AddScoped<IRequestHandler<PingRequest, string>, PingRequestHandler>();
        services.AddScoped<IPipelineBehavior<PingRequest, string>, RecordingBehavior>();

        return services.BuildServiceProvider().GetRequiredService<IMediator>();
    }

    [Fact]
    public async Task Send_ResolvesHandlerAndReturnsResponse()
    {
        var mediator = BuildMediator();

        var response = await mediator.Send(new PingRequest("hello"));

        response.Should().Be("pong:hello");
    }

    [Fact]
    public async Task Send_RunsPipelineBehaviorsAroundTheHandler()
    {
        RecordingBehavior.ExecutionOrder.Clear();
        var mediator = BuildMediator();

        await mediator.Send(new PingRequest("hello"));

        RecordingBehavior.ExecutionOrder.Should().Equal("before", "after");
    }
}
